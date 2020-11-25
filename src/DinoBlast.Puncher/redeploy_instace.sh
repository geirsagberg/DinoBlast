#! /usr/bin/env bash

# Stop, delete, and redeploy VM
# TODO: Fiks at vi lagar ein ny instans ved redeploy, fordi dette gir ny IP


gcloud compute instances stop dinoblast-puncher --project dinoblast
gcloud compute instances delete dinoblast-puncher --project dinoblast

gcloud compute instances create dinoblast-puncher --source-instance-template dinoblast-puncher --zone europe-west3-c --project dinoblast


