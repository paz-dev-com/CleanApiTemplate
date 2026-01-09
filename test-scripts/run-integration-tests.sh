#!/bin/bash
# Run integration tests only (requires SQL Server)
# Slower execution (~60-70 seconds)

set -e

echo "?? Running Integration Tests (requires SQL Server)..."
echo ""

TEST_PROJECT="../CleanApiTemplate.Test/CleanApiTemplate.Test.csproj"
FILTER="Category=Integration"

# Parse arguments
COVERAGE=""
VERBOSITY="minimal"

while [[ $# -gt 0 ]]; do
    case $1 in
        --coverage)
            COVERAGE="--collect:\"XPlat Code Coverage\""
            echo "?? Code coverage enabled"
            shift
            ;;
        --verbose)
            VERBOSITY="detailed"
            shift
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo "Command: dotnet test \"$TEST_PROJECT\" --filter \"$FILTER\" --verbosity $VERBOSITY $COVERAGE"
echo ""

# Execute tests
START_TIME=$(date +%s)
dotnet test "$TEST_PROJECT" --filter "$FILTER" --verbosity $VERBOSITY $COVERAGE
EXIT_CODE=$?
END_TIME=$(date +%s)
DURATION=$((END_TIME - START_TIME))

echo ""
if [ $EXIT_CODE -eq 0 ]; then
    echo "? Integration tests passed! Duration: ${DURATION}s"
    
    if [ -n "$COVERAGE" ]; then
        echo ""
        echo "?? Coverage reports generated in TestResults folder"
        echo "To view HTML report, run: ./generate-coverage-report.sh"
    fi
else
    echo "? Integration tests failed!"
    exit $EXIT_CODE
fi

exit 0
