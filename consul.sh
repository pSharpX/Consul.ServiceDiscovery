
# Configure and run a Consul server
docker run -d -p 8500:8500 -p 8600:8600/udp --name=<container-name> consul agent -server -ui -node=server-1 -bootstrap-expect=1 -client=0.0.0.0
docker run -d -p 8500:8500 -p 8600:8600/udp --name=my-consul-server consul agent -server -ui -node=server-1 -bootstrap-expect=1 -client=0.0.0.0

# Discover the server IP address
docker exec <consul-server-container-name> consul members
docker exec my-consul-server consul members

# Configure and run a Consul client
docker run --name=<container-name> consul agent -node=client-1 -join=<consul-server-ip>
docker run --name=my-consul-client consul agent -node=client-1 -join=172.17.0.2

# Register a service
docker pull hashicorp/counting-service:0.0.2

# 1. Run service container
docker run -p 9001:9001 -d --name=<container-name> hashicorp/counting-service:0.0.2
docker run -p 9001:9001 -d --name=weasel hashicorp/counting-service:0.0.2

# 2. you will register the counting service with the Consul client by adding a service definition file called counting.json in the directory consul/config
docker exec <consul-client-container-name> /bin/sh -c "echo '{\"service\": {\"name\": \"counting\", \"tags\": [\"go\"], \"port\": 9001}}' >> /consul/config/counting.json"
docker exec my-consul-client /bin/sh -c "echo '{\"service\": {\"name\": \"counting\", \"tags\": [\"go\"], \"port\": 9001}}' >> /consul/config/counting.json"

# 3. Since the Consul client does not automatically detect changes in the configuration directory, you will need to issue a reload command for the same container.
docker exec <consul-client-container-name> consul reload
docker exec fox consul reload

# Use Consul DNS to discover the counting service
dig @127.0.0.1 -p 8600 counting.service.consul