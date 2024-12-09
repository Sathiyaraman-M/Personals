#!/bin/bash

# Custom Echo Function
function printLog() {
  Text=$1
  NOW=$(date +"%Y-%m-%d %H:%M:%S")
  APP_NAME="Expensive"
  echo -e "\033[1;32m$APP_NAME | $NOW: $Text\033[0m"
}

# Set your image name
IMAGE="ghcr.io/sathiyaraman-m/personals:latest"
COMPOSE_DIR="$HOME/compose/personals/"

# Navigate to the Docker Compose directory
cd "$COMPOSE_DIR" || exit

# Pull the latest image
docker pull $IMAGE

# Get the running container ID
CONTAINER_ID=$(docker ps -q --filter ancestor=$IMAGE)

# Check if the container is running
if [ -n "$CONTAINER_ID" ]; then
  # Get the current image ID of the running container
  CURRENT_IMAGE=$(docker inspect --format '{{.Image}}' "$CONTAINER_ID")

  # Get the tag of the currently running image
  CURRENT_TAG=$(docker inspect --format '{{ .Config.Image }}' "$CONTAINER_ID" | awk -F':' '{print $2}')

  # Check if the container is running the latest image
  if [ "$CURRENT_TAG" == "latest" ]; then
    # Check if the image is updated
    UPDATED=$(docker images --no-trunc --quiet $IMAGE | awk -F':' '{print $2}')

    # Compare the current image ID with the updated image ID
    if [ "$CURRENT_IMAGE" != "$UPDATED" ]; then
      printLog "New image available. Updating container..."
      docker-compose pull && docker-compose up -d
      printLog "Container updated."
    else
      printLog "Container is already running the latest image."
    fi
  else
    printLog "Container is running a specific version ($CURRENT_TAG). Skipping update."
  fi
else
  printLog "Container is not running. Starting container..."
  docker-compose up -d
fi