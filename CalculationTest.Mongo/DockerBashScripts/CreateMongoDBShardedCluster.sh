docker-machine rm MongoSharded
echo "Creating VM on Hyper-V: MongoSharded"
docker-machine create -d hyperv --hyperv-virtual-switch "VMNat" --hyperv-disk-size "255000" --hyperv-memory 8192 MongoSharded

echo "Creating bridge network on MongoSharded: mongo-net"
eval $(docker-machine env MongoSharded)
docker network create mongo-net

echo "Pulling MongoDB"
eval $(docker-machine env MongoSharded)
docker pull mongo

echo "Creating Shard Servers on MongoSharded"
eval $(docker-machine env MongoSharded)
docker run -P --name=rs1-srv1  --expose=27017 --net=mongo-net  --hostname=rs1-srv1 --env="constraint:node==rs1-srv1" -d mongo --shardsvr --replSet rs1 --noprealloc --smallfiles --port 27017
eval $(docker-machine env MongoSharded)
docker run -P --name=rs1-srv2  --expose=27017 --net=mongo-net  --hostname=rs1-srv2 --env="constraint:node==rs1-srv2" -d mongo --shardsvr --replSet rs1 --noprealloc --smallfiles --port 27017
eval $(docker-machine env MongoSharded)
docker run -P --name=rs1-srv3  --expose=27017 --net=mongo-net  --hostname=rs1-srv3 --env="constraint:node==rs1-srv3" -d mongo --shardsvr --replSet rs1 --noprealloc --smallfiles --port 27017
eval $(docker-machine env MongoSharded)
docker run -P --name=rs2-srv1  --expose=27017 --net=mongo-net  --hostname=rs2-srv1 --env="constraint:node==rs2-srv1" -d mongo --shardsvr --replSet rs2 --noprealloc --smallfiles --port 27017
eval $(docker-machine env MongoSharded)
docker run -P --name=rs2-srv2  --expose=27017 --net=mongo-net  --hostname=rs2-srv2 --env="constraint:node==rs2-srv2" -d mongo --shardsvr --replSet rs2 --noprealloc --smallfiles --port 27017
eval $(docker-machine env MongoSharded)
docker run -P --name=rs2-srv3  --expose=27017 --net=mongo-net  --hostname=rs2-srv3 --env="constraint:node==rs2-srv3" -d mongo --shardsvr --replSet rs2 --noprealloc --smallfiles --port 27017

echo "Adding Replicas for rs1 Set"
eval $(docker-machine env MongoSharded)
docker exec -i rs1-srv1 mongo << EOF 
sleep(10000)
use admin
rs.initiate()
sleep(5000)

print("Adding replica member rs1-srv2")
rs.add("rs1-srv2:27017")

sleep(5000)
print("Adding replica member rs1-srv3")
rs.add("rs1-srv3:27017")

sleep(5000)
print("Status:")
rs.conf()
sleep(10000)
quit()
EOF

echo "Adding Replicas for rs2 Set"
eval $(docker-machine env MongoSharded)
docker exec -i rs2-srv1 mongo << EOF 
sleep(10000)
use admin
rs.initiate()
sleep(5000)

print("Adding replica member rs2-srv2")
rs.add("rs2-srv2:27017")

sleep(5000)
print("Adding replica member rs2-srv3")
rs.add("rs2-srv3:27017")

sleep(5000)
print("Status:")
rs.conf()
sleep(10000)
quit()
EOF

echo "Creating Configuration Servers on MongoSharded"

docker-machine ssh MongoSharded  "docker run -P --name=cfg1 --net=mongo-net --hostname=cfg1 --expose=27017 --env="constraint:node==cfg1"  -d mongo  --port 27017 --noprealloc --smallfiles --configsvr --replSet confs  --dbpath /data/db"

docker-machine ssh MongoSharded "docker run -P --name=cfg2 --net=mongo-net --hostname=cfg2 --expose=27017 --env="constraint:node==cfg2"  -d mongo  --port 27017 --noprealloc --smallfiles --configsvr --replSet confs  --dbpath /data/db"

docker-machine ssh MongoSharded "docker run -P --name=cfg3 --net=mongo-net --hostname=cfg3 --expose=27017 --env="constraint:node==cfg3"  -d mongo  --port 27017 --noprealloc --smallfiles --configsvr --replSet confs  --dbpath /data/db"

echo "Adding Replicas for conf Set"
eval $(docker-machine env MongoSharded)
docker exec -i cfg1 mongo << EOF 
sleep(10000)
use admin
rs.initiate()
sleep(5000)

print("Adding replica member cfg2")
rs.add("cfg2:27017")

sleep(5000)
print("Adding replica member cfg3")
rs.add("cfg3:27017")

sleep(5000)
print("Status:")
rs.conf()
sleep(10000)
quit()
EOF

echo "Creating MongoDB router on MongoSharded"
eval $(docker-machine env MongoSharded)
docker run -P --name=mongo-router --expose=27017 --env="constraint:node==mongo-router" --hostname=mongo-router --net=mongo-net -d mongo mongos --configdb confs/cfg1:27017,cfg2:27017,cfg3:27017 --port 27017

echo "Adding Shards & Creating sample database into MongoDB Router"
eval $(docker-machine env MongoSharded)
docker exec -i mongo-router mongo << EOF 

print("Adding rs1/rs1-srv1:27017")
sh.addShard("rs1/rs1-srv1:27017")
sleep(5000)

print("Adding rs1/rs1-srv2:27017")
sh.addShard("rs1/rs1-srv2:27017")
sleep(5000)

print("Adding rs1/rs1-srv3:27017")
sh.addShard("rs1/rs1-srv3:27017")
sleep(5000)

print("Adding rs2/rs2-srv1:27017")
sh.addShard("rs2/rs2-srv1:27017")
sleep(5000)

print("Adding rs2/rs2-srv2:27017")
sh.addShard("rs2/rs2-srv2:27017")
sleep(5000)

print("Adding rs2/rs2-srv3:27017")
sh.addShard("rs2/rs2-srv3:27017")
sleep(10000)

print("Creating default admin user: mongoadmin:12345")
use admin
db.createUser({ user: 'mongoadmin', pwd: '12345', roles: [ { role: "userAdminAnyDatabase", db: "admin" } ] });

print("Creating test database CalculationTest")
use CalculationTest
sh.enableSharding("CalculationTest")
sleep(5000)

print("Creating accounts collection")
db.accounts.ensureIndex( { _id : 1 } )
db.accounts.ensureIndex( { ShardKey : "hashed" } )
sh.shardCollection("CalculationTest.accounts", { "ShardKey": "hashed" })
sh.enableBalancing("CalculationTest.accounts") 
sleep(5000)

print("Creating account factors collection")
db.accountfactors.ensureIndex( { ShardKey : "hashed" } )
db.accountfactors.ensureIndex( { account_id : 1 } )
db.accountfactors.ensureIndex( { _id : 1 } )
sh.shardCollection("CalculationTest.accountfactors", { "ShardKey": "hashed" })
sh.enableBalancing("CalculationTest.accountfactors")  
sleep(5000)

print("Starting balancer")
sh.startBalancer() 
sleep(10000)
quit()
EOF

echo "****************************"
echo "MongoDB Router for Sharded Cluster: "
echo "$(docker-machine ip MongoSharded):$(docker inspect --format '{{ (index (index .NetworkSettings.Ports "27017/tcp") 0).HostPort }}' mongo-router)"
echo "****************************"
echo "done."
echo "****************************"


