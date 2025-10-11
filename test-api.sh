#!/bin/bash

echo "Starting Backend API..."
cd "/Users/orxanmusayev/Projects - Important/Salymed/backend"

# Kill existing process
lsof -ti:5208 | xargs kill -9 2>/dev/null
sleep 2

# Start backend
dotnet run &
BACKEND_PID=$!

echo "Backend started with PID: $BACKEND_PID"
echo "Waiting for backend to be ready..."
sleep 6

# Test ClinicTypes API
echo ""
echo "Testing ClinicTypes API..."
curl -s http://localhost:5208/api/ClinicTypes | python3 -m json.tool

echo ""
echo ""
echo "Backend is running on http://localhost:5208"
echo "To stop: kill $BACKEND_PID"
