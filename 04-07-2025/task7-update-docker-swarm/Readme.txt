docker service create \
  --name nginx-web \
  --replicas 3 \
  --publish 8080:80 \
  nginx:alpine

u36jbj5qzrwmtbbybthhycvvr
overall progress: 3 out of 3 tasks 
1/3: running   [==================================================>] 
2/3: running   [==================================================>] 
3/3: running   [==================================================>] 
verify: Service u36jbj5qzrwmtbbybthhycvvr converged 


docker service ls

ID             NAME        MODE         REPLICAS   IMAGE          PORTS
u36jbj5qzrwm   nginx-web   replicated   3/3        nginx:latest   *:8080->80/tcp


docker service update --image nginx:alpine nginx-web

nginx-web
overall progress: 3 out of 3 tasks 
1/3: running   [==================================================>] 
2/3: running   [==================================================>] 
3/3: running   [==================================================>] 


docker service ps nginx-web

ID             NAME              IMAGE          NODE             DESIRED STATE   CURRENT STATE             ERROR     PORTS
oob6ev6a9mv0   nginx-web.1       nginx:alpine   docker-desktop   Running         Running 31 seconds ago              
mbkfjrfkdt3q    \_ nginx-web.1   nginx:latest   docker-desktop   Shutdown        Shutdown 32 seconds ago             
9wiczb9uewyk   nginx-web.2       nginx:alpine   docker-desktop   Running         Running 24 seconds ago              
hq0wlpxir85b    \_ nginx-web.2   nginx:latest   docker-desktop   Shutdown        Shutdown 24 seconds ago             
st5fg8hie1jy   nginx-web.3       nginx:alpine   docker-desktop   Running         Running 28 seconds ago              
zhvqmgsecttd    \_ nginx-web.3   nginx:latest   docker-desktop   Shutdown        Shutdown 28 seconds ago             


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