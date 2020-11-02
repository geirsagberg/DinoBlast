#! /usr/bin/env bash

# Stop, delete, and redeploy VM

gcloud compute instances stop dinoblast-puncher
gcloud compute instances delete dinoblast-puncher

gcloud compute instances create dinoblast-puncher --source-instance-template dinoblast-puncher --zone europe-west3-c


