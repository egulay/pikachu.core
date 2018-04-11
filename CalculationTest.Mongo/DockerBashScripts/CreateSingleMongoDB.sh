docker-machine rm Mongo

echo "Creating VM on Hyper-V: Mongo"
docker-machine create -d hyperv --hyperv-virtual-switch "VMNat" --hyperv-disk-size "55000" --hyperv-memory 8192 Mongo

echo "Pulling MongoDB"
eval $(docker-machine env Mongo)
docker pull mongo

echo "Creating MongoDB Container on VM: Mongo"
eval $(docker-machine env Mongo)
docker run -P --name=mongodb  --expose=27017  --hostname=mongodb --env="constraint:node==mongodb" -d mongo --port 27017

eval $(docker-machine env Mongo)
docker exec -i mongodb mongo << EOF 
sleep(5000)
print("Creating default admin user: mongoadmin:12345")
use admin
db.createUser({ user: 'mongoadmin', pwd: '12345', roles: [ { role: "userAdminAnyDatabase", db: "admin" } ] });
sleep(5000)

print("Creating accounts collection")
use CalculationTest
db.accounts.ensureIndex( { _id : 1 } )
sleep(5000)

print("Creating account factors collection")
use CalculationTest
db.accountfactors.ensureIndex( { _id : 1 } )
db.accountfactors.ensureIndex( { account_id : 1 } )
sleep(5000)

quit()
EOF

echo "****************************"
echo "Single MongoDB on VM: Mongo: "
echo "$(docker-machine ip Mongo):$(docker inspect --format '{{ (index (index .NetworkSettings.Ports "27017/tcp") 0).HostPort }}' mongodb)"
echo "****************************"
echo "done."
echo "****************************"