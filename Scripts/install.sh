#!/bin/bash

function execute {
	if ! "$@"; then
		echo "An error occured."
		exit 1
	fi
}


if [ "$#" -ne 0 ]; then
	echo "Waiting for the bot to shutdown... "
	wait "$1"
fi

rm Nami*.zip &2> /dev/null

echo "Downloading ... "
execute wget "https://ci.appveyor.com/api/projects/Thalitech/Nami/artifacts/Nami.zip" -q --show-progress
execute wget "https://ci.appveyor.com/api/projects/Thalitech/Nami/artifacts/NamiResources.zip" -q --show-progress

echo "Extracting ... "
execute unzip -o Nami.zip
mkdir -p Resources
execute unzip -o NamiResources.zip -d Resources/

echo "Starting the bot... "
execute dotnet Nami.dll
