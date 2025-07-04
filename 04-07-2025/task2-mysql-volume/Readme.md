docker volume create mydbdata

docker run -d --name mysql-test \
  -e MYSQL_ROOT_PASSWORD=rootpass \
  -e MYSQL_DATABASE=testdb \
  -v mydbdata:/var/lib/mysql \
  -p 3306:3306 \
  mysql:latest

  docker exec -it mysql-test mysql -uroot -prootpass

  USE testdb;
CREATE TABLE users (id INT AUTO_INCREMENT PRIMARY KEY, name VARCHAR(255));
INSERT INTO users (name) VALUES ('Alice');

docker stop mysql-test && docker rm mysql-test


docker run -d --name mysql-test \
  -e MYSQL_ROOT_PASSWORD=rootpass \
  -e MYSQL_DATABASE=testdb \
  -v mydbdata:/var/lib/mysql \
  -p 3306:3306 \
  mysql:latest

docker exec -it mysql-test mysql -uroot -prootpass -e "SELECT * FROM testdb.users;"


