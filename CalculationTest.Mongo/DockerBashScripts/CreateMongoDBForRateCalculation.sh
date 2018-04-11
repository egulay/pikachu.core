docker-machine rm MongoCurrency

echo "Creating VM on Hyper-V: Mongo"
docker-machine create -d hyperv --hyperv-virtual-switch "VMNat" --hyperv-disk-size "55000" --hyperv-memory 8192 MongoCurrency

echo "Pulling MongoDB"
eval $(docker-machine env MongoCurrency)
docker pull mongo

echo "Creating MongoDB Container on VM: Mongo"
eval $(docker-machine env MongoCurrency)
docker run -P --name=mongodb  --expose=27017  --hostname=mongodb --env="constraint:node==mongodb" -d mongo --port 27017

eval $(docker-machine env MongoCurrency)
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

print("Creating rates collection")
use CalculationTest
db.rates.ensureIndex( { _id : 1 } )
sleep(5000)

print("Creating conversion results collection")
use CalculationTest
db.results.ensureIndex( { _id : 1 } )
sleep(5000)

print("Creating result clone collection")
use CalculationTest
db.resultclones.ensureIndex( { _id : 1 } )
sleep(5000)

quit()
EOF

echo "****************************"
echo "Single MongoDB on VM: MongoCurrency: "
echo "$(docker-machine ip MongoCurrency):$(docker inspect --format '{{ (index (index .NetworkSettings.Ports "27017/tcp") 0).HostPort }}' mongodb)"
echo "****************************"
echo "done."
echo "****************************"