#! /usr/bin/env bash

docker build -t eu.gcr.io/dinoblast/puncher .

docker push eu.gcr.io/dinoblast/puncher:latest
