#!/usr/bin/env bash
set -e

echo "Packing mrGitTags..."
dotnet pack -c Release -o nupkg

echo "Updating global tool..."
dotnet tool update --global mrGitTags --add-source ./nupkg

echo "Done! mrGitTags has been updated."
