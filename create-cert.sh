#!/bin/bash

# Sertifika oluştur
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes -subj "/CN=localhost"

# PFX formatına dönüştür
openssl pkcs12 -export -out cert.pfx -inkey key.pem -in cert.pem -passout pass:password

# Geçici dosyaları temizle
rm key.pem cert.pem

# Sertifikayı volume'a kopyala
docker volume create cert_volume
docker run --rm -v cert_volume:/cert -v $(pwd):/source alpine sh -c "cp /source/cert.pfx /cert/cert.pfx" 