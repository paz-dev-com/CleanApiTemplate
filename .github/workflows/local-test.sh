#!/bin/bash

# CI/CD Pipeline - Local Test Runner
# This script simulates the GitHub Actions pipeline locally

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
DOTNET_VERSION="8.0"
SOLUTION_PATH="./CleanApiTemplate.sln"
TEST_PROJECT="./CleanApiTemplate.Test/CleanApiTemplate.Test.csproj"

# Helper functions
print_header() {
    echo ""
    echo -e "${BLUE}============================================${NC}"
    echo -e "${BLUE} $1${NC}"
    echo -e "${BLUE}============================================${NC}"
    echo ""
}

print_success() {
    echo -e "${GREEN}[PASS] $1${NC}"
}

print_error() {
    echo -e "${RED}[FAIL] $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}[WARN] $1${NC}"
}

print_info() {
    echo -e "${BLUE}[INFO] $1${NC}"
}

# Check prerequisites
check_prerequisites() {
    print_header "Checking Prerequisites"
    
    # Check .NET SDK
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK not found. Please install .NET $DOTNET_VERSION SDK"
        exit 1
    fi
    
    dotnet_version=$(dotnet --version)
    print_success ".NET SDK found: $dotnet_version"
    
    # Check for solution file
    if [ ! -f "$SOLUTION_PATH" ]; then
        print_error "Solution file not found: $SOLUTION_PATH"
        exit 1
    fi
    print_success "Solution file found"
    
    # Check for test project
    if [ ! -f "$TEST_PROJECT" ]; then
        print_error "Test project not found: $TEST_PROJECT"
        exit 1
    fi
    print_success "Test project found"
}

# Restore dependencies
restore_dependencies() {
    print_header "Restoring Dependencies"
    
    dotnet restore "$SOLUTION_PATH"
    
    if [ $? -eq 0 ]; then
        print_success "Dependencies restored successfully"
    else
        print_error "Failed to restore dependencies"
        exit 1
    fi
}

# Build solution
build_solution() {
    print_header "Building Solution"
    
    dotnet build "$SOLUTION_PATH" --configuration Release --no-restore
    
    if [ $? -eq 0 ]; then
        print_success "Build completed successfully"
    else
        print_error "Build failed"
        exit 1
    fi
}

# Check code formatting
check_formatting() {
    print_header "Checking Code Formatting"
    
    # Install dotnet format if not already installed
    if ! dotnet format --version &> /dev/null; then
        print_info "Installing dotnet format..."
        dotnet tool install -g dotnet-format
    fi
    
    dotnet format "$SOLUTION_PATH" --verify-no-changes --verbosity diagnostic
    
    if [ $? -eq 0 ]; then
        print_success "Code formatting is correct"
    else
        print_warning "Code formatting issues detected. Run 'dotnet format' to fix"
        return 1
    fi
}

# Run unit tests
run_unit_tests() {
    print_header "Running Unit Tests"
    
    dotnet test "$TEST_PROJECT" \
        --configuration Release \
        --no-build \
        --filter "Category!=Integration" \
        --logger "console;verbosity=detailed" \
        --collect:"XPlat Code Coverage" \
        --results-directory ./TestResults/Unit
    
    if [ $? -eq 0 ]; then
        print_success "Unit tests passed"
    else
        print_error "Unit tests failed"
        exit 1
    fi
}

# Run integration tests
run_integration_tests() {
    print_header "Running Integration Tests"
    
    # Check if SQL Server is accessible
    print_info "Checking SQL Server connectivity..."
    
    # Try to connect to SQL Server (adjust connection details as needed)
    if command -v sqlcmd &> /dev/null; then
        if sqlcmd -S localhost -U sa -P "P@ssw0rd123!" -Q "SELECT 1" &> /dev/null; then
            print_success "SQL Server is accessible"
        else
            print_warning "SQL Server not accessible. Skipping integration tests"
            print_info "To run integration tests, ensure SQL Server is running"
            return 0
        fi
    else
        print_warning "sqlcmd not found. Cannot verify SQL Server. Skipping integration tests"
        return 0
    fi
    
    # Set connection string
    export ConnectionStrings__TestConnection="Server=localhost,1433;Database=CleanApiTemplate_Test;User ID=sa;Password=P@ssw0rd123!;TrustServerCertificate=True;MultipleActiveResultSets=True;Connection Timeout=30;"
    
    dotnet test "$TEST_PROJECT" \
        --configuration Release \
        --no-build \
        --filter "Category=Integration" \
        --logger "console;verbosity=detailed" \
        --collect:"XPlat Code Coverage" \
        --results-directory ./TestResults/Integration
    
    if [ $? -eq 0 ]; then
        print_success "Integration tests passed"
    else
        print_error "Integration tests failed"
        exit 1
    fi
}

# Generate coverage report
generate_coverage_report() {
    print_header "Generating Coverage Report"
    
    # Install reportgenerator if not already installed
    if ! dotnet reportgenerator --version &> /dev/null 2>&1; then
        print_info "Installing ReportGenerator..."
        dotnet tool install -g dotnet-reportgenerator-globaltool
    fi
    
    # Check if coverage files exist
    if [ ! -d "TestResults" ]; then
        print_warning "No test results found. Skipping coverage report"
        return 0
    fi
    
    # Find all coverage files
    coverage_files=$(find TestResults -name "coverage.cobertura.xml" 2>/dev/null)
    
    if [ -z "$coverage_files" ]; then
        print_warning "No coverage files found. Skipping coverage report"
        return 0
    fi
    
    # Generate report
    dotnet reportgenerator \
        -reports:"TestResults/**/coverage.cobertura.xml" \
        -targetdir:"coveragereport" \
        -reporttypes:"Html;TextSummary" \
        -assemblyfilters:"-xunit*;-*.Tests;-*.Test"
    
    if [ $? -eq 0 ]; then
        print_success "Coverage report generated"
        
        # Display summary
        if [ -f "coveragereport/Summary.txt" ]; then
            echo ""
            cat coveragereport/Summary.txt
            echo ""
        fi
        
        print_info "Open coveragereport/index.html to view detailed report"
        
        # Check coverage threshold
        if [ -f "coveragereport/Summary.txt" ]; then
            line_coverage=$(grep "Line coverage:" coveragereport/Summary.txt | grep -oP '\d+\.\d+%' | head -1 | sed 's/%//')
            
            if [ ! -z "$line_coverage" ]; then
                threshold=70.0
                
                if (( $(echo "$line_coverage < $threshold" | bc -l) )); then
                    print_error "Coverage $line_coverage% is below threshold $threshold%"
                    return 1
                else
                    print_success "Coverage $line_coverage% meets threshold $threshold%"
                fi
            fi
        fi
    else
        print_warning "Failed to generate coverage report"
        return 1
    fi
}

# Check test completeness
check_test_completeness() {
    print_header "Checking Test Completeness"
    
    handler_count=0
    test_count=0
    missing_tests=()
    
    print_info "Analyzing handlers..."
    
    # Find all handler classes
    for handler in $(find CleanApiTemplate.Application -name "*Handler.cs" -not -path "*/obj/*" -not -path "*/bin/*" 2>/dev/null); do
        handler_name=$(basename "$handler" .cs)
        test_file="CleanApiTemplate.Test/Application/Handlers/${handler_name}Tests.cs"
        
        ((handler_count++))
        
        if [ -f "$test_file" ]; then
            test_methods=$(grep -c "public.*void.*Test\|public.*Task.*Test\|\[Fact\]\|\[Theory\]" "$test_file" 2>/dev/null || echo "0")
            print_success "$handler_name: $test_methods tests"
            ((test_count++))
        else
            print_error "$handler_name: No test file found!"
            missing_tests+=("$handler_name")
        fi
    done
    
    print_info "Analyzing validators..."
    
    validator_count=0
    validator_test_count=0
    
    # Find all validator classes
    for validator in $(find CleanApiTemplate.Application -name "*Validator.cs" -not -path "*/obj/*" -not -path "*/bin/*" 2>/dev/null); do
        validator_name=$(basename "$validator" .cs)
        test_file="CleanApiTemplate.Test/Application/Validators/${validator_name}Tests.cs"
        
        ((validator_count++))
        
        if [ -f "$test_file" ]; then
            test_methods=$(grep -c "public.*void.*Test\|public.*Task.*Test\|\[Fact\]\|\[Theory\]" "$test_file" 2>/dev/null || echo "0")
            print_success "$validator_name: $test_methods tests"
            ((validator_test_count++))
        else
            print_error "$validator_name: No test file found!"
            missing_tests+=("$validator_name")
        fi
    done
    
    echo ""
    print_info "Summary:"
    echo "  Handlers: $test_count/$handler_count"
    echo "  Validators: $validator_test_count/$validator_count"
    
    total_classes=$((handler_count + validator_count))
    total_tested=$((test_count + validator_test_count))
    
    if [ $total_classes -gt 0 ]; then
        completeness=$(echo "scale=2; $total_tested * 100 / $total_classes" | bc)
        echo "  Overall: $completeness%"
        echo ""
        
        if [ ${#missing_tests[@]} -gt 0 ]; then
            print_error "Missing tests for ${#missing_tests[@]} classes"
            return 1
        else
            print_success "All classes have tests!"
        fi
        
        if (( $(echo "$completeness < 80" | bc -l) )); then
            print_error "Test completeness $completeness% is below 80%"
            return 1
        else
            print_success "Test completeness $completeness% meets threshold"
        fi
    fi
}

# Security scan
security_scan() {
    print_header "Running Security Scan"
    
    dotnet list package --vulnerable --include-transitive 2>&1 | tee security-scan.txt
    
    if grep -q "has the following vulnerable packages" security-scan.txt; then
        print_error "Vulnerable packages found!"
        return 1
    else
        print_success "No vulnerable packages found"
        rm -f security-scan.txt
    fi
}

# Main execution
main() {
    clear
    echo ""
    echo -e "${BLUE}============================================${NC}"
    echo -e "${BLUE}   CI/CD Pipeline - Local Test Runner     ${NC}"
    echo -e "${BLUE}============================================${NC}"
    echo ""
    
    start_time=$(date +%s)
    
    # Track failures
    failures=0
    
    # Run all checks
    check_prerequisites || ((failures++))
    restore_dependencies || ((failures++))
    build_solution || ((failures++))
    check_formatting || ((failures++))
    run_unit_tests || ((failures++))
    run_integration_tests || ((failures++))
    generate_coverage_report || ((failures++))
    check_test_completeness || ((failures++))
    security_scan || ((failures++))
    
    # Calculate execution time
    end_time=$(date +%s)
    duration=$((end_time - start_time))
    
    # Final summary
    echo ""
    print_header "Pipeline Summary"
    
    if [ $failures -eq 0 ]; then
        print_success "All checks passed!"
        echo ""
        print_info "Pipeline completed in ${duration}s"
        exit 0
    else
        print_error "$failures check(s) failed"
        echo ""
        print_info "Pipeline completed in ${duration}s"
        exit 1
    fi
}

# Parse command line arguments
case "${1:-all}" in
    prereq)
        check_prerequisites
        ;;
    restore)
        restore_dependencies
        ;;
    build)
        build_solution
        ;;
    format)
        check_formatting
        ;;
    unit)
        run_unit_tests
        ;;
    integration)
        run_integration_tests
        ;;
    coverage)
        generate_coverage_report
        ;;
    completeness)
        check_test_completeness
        ;;
    security)
        security_scan
        ;;
    all)
        main
        ;;
    *)
        echo "Usage: $0 {all|prereq|restore|build|format|unit|integration|coverage|completeness|security}"
        echo ""
        echo "Commands:"
        echo "  all          - Run all checks (default)"
        echo "  prereq       - Check prerequisites only"
        echo "  restore      - Restore dependencies only"
        echo "  build        - Build solution only"
        echo "  format       - Check code formatting only"
        echo "  unit         - Run unit tests only"
        echo "  integration  - Run integration tests only"
        echo "  coverage     - Generate coverage report only"
        echo "  completeness - Check test completeness only"
        echo "  security     - Run security scan only"
        exit 1
        ;;
esac
