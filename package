#!/bin/bash

function check_file_exists() {
    if ! [ -e "$1" ]; then
        echo "Failed to find $1!"
        exit 1
    fi
}

INFO_FILE="info.json"
MAPIFY_DLL="build/Mapify.dll"
MAPIFY_EDITOR_DLL="build/MapifyEditor.dll"
DISPLAY_NAME=$(jq -r '.DisplayName' $INFO_FILE)
VERSION=$(jq -r '.Version' $INFO_FILE)

check_file_exists "$MAPIFY_DLL"
check_file_exists "$MAPIFY_EDITOR_DLL"
check_file_exists "$INFO_FILE"

zip -1 -T -j -u "${DISPLAY_NAME}_v$VERSION.zip" $MAPIFY_DLL $MAPIFY_EDITOR_DLL $INFO_FILE
