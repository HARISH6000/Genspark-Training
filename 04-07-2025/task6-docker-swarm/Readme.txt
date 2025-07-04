docker swarm init

Swarm initialized: current node (uzq9zn42a809h6gnth9jkt7uw) is now a manager.

To add a worker to this swarm, run the following command:

    docker swarm join --token SWMTKN-1-2snkwctev527e3ulecqtysnhs368ijsb4vi3sd2mbb2u1w0845-avi7385egjs2e7mqe09lt09n7 192.168.65.3:2377


docker service create \
  --name nginx-web \
  --replicas 3 \
  --publish 8080:80 \
  nginx:alpine


5nd7s72c7uqsc97yjrotigqmz
overall progress: 3 out of 3 tasks 
1/3: running   [==================================================>] 
2/3: running   [==================================================>] 
3/3: running   [==================================================>] 
verify: Service 5nd7s72c7uqsc97yjrotigqmz converged 


docker service ls

ID             NAME        MODE         REPLICAS   IMAGE          PORTS
5nd7s72c7uqs   nginx-web   replicated   3/3        nginx:alpine   *:8080->80/tcp


docker ps

CONTAINER ID   IMAGE          COMMAND                  CREATED         STATUS         PORTS     NAMES
b02440025ecd   nginx:alpine   "/docker-entrypoint.…"   4 minutes ago   Up 4 minutes   80/tcp    nginx-web.3.qiazimnt5kv4w9mfiym350xt1
a99da9b6fc55   nginx:alpine   "/docker-entrypoint.…"   4 minutes ago   Up 4 minutes   80/tcp    nginx-web.1.0nvmcm3yapuval02vrrbf4s35
0b3c9aec610f   nginx:alpine   "/docker-entrypoint.…"   4 minutes ago   Up 4 minutes   80/tcp    nginx-web.2.xt46ozajwmsm52ccro063twc0


curl localhost:8080

<!DOCTYPE html>
<html>
<head>
<title>Welcome to nginx!</title>
<style>
html { color-scheme: light dark; }
body { width: 35em; margin: 0 auto;
font-family: Tahoma, Verdana, Arial, sans-serif; }
</style>
</head>
<body>
<h1>Welcome to nginx!</h1>
<p>If you see this page, the nginx web server is successfully installed and
working. Further configuration is required.</p>

<p>For online documentation and support please refer to
<a href="http://nginx.org/">nginx.org</a>.<br/>
Commercial support is available at
<a href="http://nginx.com/">nginx.com</a>.</p>

<p><em>Thank you for using nginx.</em></p>
</body>
</html>


docker service rm nginx-web

nginx-web
