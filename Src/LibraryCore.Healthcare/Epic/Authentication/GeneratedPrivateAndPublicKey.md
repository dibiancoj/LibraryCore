### To generate a private and public key for epic.
```openssl genrsa -out privatekey.pem 2048```
```openssl req -new -x509 -key privatekey.pem -out publickey509.pem -subj '/CN=myapp'```