@echo off

:: 生成私钥 + 自签名证书（一次完成）
openssl req -x509 -newkey rsa:2048 -keyout server.key -out server.crt -days 3650 -nodes -config san.cnf -extensions v3_req

:: 生成pfx供服务器使用（密码自己设）
openssl pkcs12 -export -out server.pfx -inkey server.key -in server.crt -passout pass:DGQU4zgMwNSw2hUx

::echo DGQU4zgMwNSw2hUx > password.txt