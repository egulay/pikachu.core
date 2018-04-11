docker-machine rm Mongo0
docker-machine rm Mongo1
docker-machine rm Mongo2

echo "Starting Consul Server on localhost"
docker rm $(docker ps -a -q) -f
docker run -d \
     -p "8500:8500" \
     -h "consul" \
     progrium/consul -server -bootstrap

echo "Creating MongoDB Cluster VMs"
docker-machine create -d hyperv --hyperv-virtual-switch "VMNat" --hyperv-disk-size "55000" --hyperv-memory 1024 \
--swarm --swarm-master \
--swarm-discovery="consul://10.4.160.172:8500" \
--engine-opt="cluster-store=consul://10.4.160.172:8500" \
--engine-opt="cluster-advertise=eth0:2376" Mongo0

docker-machine create -d hyperv --hyperv-virtual-switch "VMNat" --hyperv-disk-size "55000" --hyperv-memory 1024 \
--swarm \
--swarm-discovery="consul://10.4.160.172:8500" \
--engine-opt="cluster-store=consul://10.4.160.172:8500" \
--engine-opt="cluster-advertise=eth0:2376" Mongo1

docker-machine create -d hyperv --hyperv-virtual-switch "VMNat" --hyperv-disk-size "55000" --hyperv-memory 1024 \
--swarm \
--swarm-discovery="consul://10.4.160.172:8500" \
--engine-opt="cluster-store=consul://10.4.160.172:8500" \
--engine-opt="cluster-advertise=eth0:2376" Mongo2

echo "Creating Network Interface: mongonet"
eval $(docker-machine env --swarm Mongo0)
docker network create --driver overlay mongonet

echo "Starting MongoDB Containers"
docker-machine ssh Mongo0 "docker run -d --name=mongo0 --hostname mongo0 -p 27017:27017 --net=mongonet --env="constraint:node==Mongo0" mongo --replSet "mongoDB-cluster" --port 27017"

docker-machine ssh Mongo1 "docker run -d --name=mongo1 --hostname mongo1 -p 27018:27018 --net=mongonet --env="constraint:node==Mongo1" mongo --replSet "mongoDB-cluster" --port 27018"

docker-machine ssh Mongo2 "docker run -d --name=mongo2 --hostname mongo2 -p 27019:27019 --net=mongonet --env="constraint:node==Mongo2" mongo --replSet "mongoDB-cluster" --port 27019"

echo "Adding replicas into MongoDB Cluster"
eval $(docker-machine env --swarm Mongo0)
docker exec -i mongo0 mongo << EOF 
use admin
rs.initiate()
sleep(5000)

print("Adding replica member mongo1")
rs.add("mongo1:27018")

sleep(5000)
print("Adding replica member mongo2")
rs.add("mongo2:27019")

sleep(5000)

use admin
db.createUser({ user: 'mongoadmin', pwd: '12345', roles: [ { role: "userAdminAnyDatabase", db: "admin" } ] });
print("Status:")
rs.conf()
sleep(10000)
quit()
EOF

echo "Starting services on Mongo0"
docker-machine ssh Mongo0 "docker start swarm-agent-master"
docker-machine ssh Mongo0 "docker start swarm-agent"
docker-machine ssh Mongo0 "docker start mongo0"

echo "Starting services on Mongo1"
docker-machine ssh Mongo1 "docker start swarm-agent"
docker-machine ssh Mongo1 "docker start mongo1"

echo "Starting services on Mongo1"
docker-machine ssh Mongo2 "docker start swarm-agent"
docker-machine ssh Mongo2 "docker start mongo2"

echo "****************************"
echo "MongoDB Cluser: "
echo "$(docker-machine ip Mongo0):27017,$(docker-machine ip Mongo1):27018,$(docker-machine ip Mongo2):27019"
echo "****************************"
echo "done."
echo "****************************"