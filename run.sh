#!/bin/sh

echo Running SFIT Discovery Microservices

set -o allexport
source ./.env
set +o allexport

#docker-compose config
docker-compose up

read -n 1 -s -r -p "Press any key to continue"