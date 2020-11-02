#! /usr/bin/env bash

# Read here to authenticate: https://cloud.google.com/sdk/gcloud/reference/auth/configure-docker

docker build -t eu.gcr.io/dinoblast/puncher .

docker push eu.gcr.io/dinoblast/puncher:latest
