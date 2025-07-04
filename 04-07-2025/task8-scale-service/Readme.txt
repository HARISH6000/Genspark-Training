docker service scale nginx-web=5

overall progress: 5 out of 5 tasks 
1/5: running   [==================================================>] 
2/5: running   [==================================================>] 
3/5: running   [==================================================>] 
4/5: running   [==================================================>] 
5/5: running   [==================================================>] 
verify: Service nginx-web converged 


docker service ls

ID             NAME        MODE         REPLICAS   IMAGE          PORTS
u36jbj5qzrwm   nginx-web   replicated   5/5        nginx:alpine   *:8080->80/tcp

