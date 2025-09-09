# AutoTestID Workflow MCP Server

A comprehensive Model Context Protocol (MCP) server implementing two-phase AutoTestID workflow for automated testing with accessibility-first approach.

## Overview

AutoTestID Workflow MCP Server provides the `autotestid_workflow` tool that implements a sophisticated two-phase approach:

1. **Phase 1: Strategy Selection** - Choose between `aria-first` or `test-attribute-first` 
2. **Phase 2: Implementation** - Apply your chosen strategy with enterprise-grade locator patterns

## Features

- **🎯 Two-Phase Workflow**: Strategy selection followed by intelligent implementation
- **♿ ARIA-First Option**: Prioritizes accessibility with semantic targeting (ARIA attributes OR data-testid, never both)
- **🎛️ Test-Attribute-First Option**: Comprehensive data-testid coverage for all interactive elements
- **🏢 Enterprise-Grade**: Follows Angular/AngularJS best practices with proper naming conventions
- **🚀 MCP Compatible**: Seamlessly integrates with VS Code and GitHub Copilot

## Project Structure

```
AutoTestId.McpServer/
├── AutoTestId.McpServer.csproj    # Project configuration
├── Program.cs                     # Application entry point  
├── McpServer.cs                   # MCP protocol implementation
├── TestIdTools.cs                 # Two-phase AutoTestID workflow
├── tech_prompts/
│   └── add_test_id_prompt.md      # Comprehensive workflow template
└── README.md                      # This guide
```

## Requirements

- .NET 8.0 SDK or later
- VS Code with MCP Server Extension
- GitHub Copilot (for chat integration)

## Quick Setup

1. **Build the Project**:
   ```bash
   dotnet build
   dotnet run
   ```

2. **Configure VS Code MCP**:
   Add to your VS Code MCP configuration:
   ```json
   {
     "servers": {
       "autotestid-workflow": {
         "type": "stdio",
         "command": "dotnet",
         "args": ["run", "--project", "path/to/AutoTestId.McpServer.csproj"]
       }
     }
   }
   ```

3. **Use the Tool**:
   - Open GitHub Copilot Chat in VS Code
   - **Option A - Direct HTML**: `#autotestid_workflow [your HTML]`
   - **Option B - File Context**: Open your HTML file in VS Code or Add it as context in the chat, then use `#autotestid_workflow` (recommended for larger files)
   - Follow the two-phase workflow prompts

## Usage

Use the tool through GitHub Copilot Chat with the two-phase workflow:

### Step 1: Trigger the Workflow

**Method A - Direct HTML Input:**
```
#autotestid_workflow Add locators to this HTML:
<div><button>Save</button><input type="email"></div>
```

**Method B - Using HTML File Context (Recommended):**
1. Open your HTML file in VS Code (e.g., `component.html`, `modal.html`)
2. Use Copilot Chat:
```
#autotestid_workflow Apply AutoTestID workflow to the current HTML file
```

This method automatically uses the open file as context, making it perfect for:
- Large HTML files or components
- Angular/React component templates  
- Complex forms and modals
- Entire page layouts

### Step 2: Choose Your Strategy
**Option A - ARIA-First (Recommended):**
```
aria-first
```
- Adds ARIA attributes for accessibility and semantic targeting
- Only uses `data-testid` when ARIA is insufficient
- Best for user-centric testing

**Option B - Test-Attribute-First:**
```
test-attribute-first
```
- Adds `data-testid` to all interactive elements
- Consistent coverage regardless of existing attributes
- Best for comprehensive automation coverage

### What You Get
- Enhanced HTML with appropriate locator attributes
- Detailed analysis of which elements were modified
- Recommendations for testing approach
- **File Integration**: When using file context, you can directly apply changes to your component files

## Tool Description

### autotestid_workflow

**Purpose**: Comprehensive AutoTestID workflow for Angular/AngularJS applications

**Two Strategies Available:**

1. **ARIA-First Strategy**
   - Prioritizes accessibility with `aria-label`, `role` attributes
   - Only adds `data-testid` when ARIA is insufficient for targeting
   - Follows the rule: ARIA OR data-testid, never both on same element

2. **Test-Attribute-First Strategy**  
   - Adds `data-testid` to all interactive elements
   - Uses kebab-case naming: `submit-button`, `email-input`
   - Ensures comprehensive test coverage

**Input**: 
- `html_content` (required): HTML content to enhance with locator attributes

**Output**: 
- Strategy selection prompt → Enhanced HTML with chosen approach
- Analysis summary with recommendations

## Example Outputs

### ARIA-First Strategy Result:
```html
<!-- Input -->
<button>Save</button>

<!-- Output -->
<button aria-label="Save form data" role="button">Save</button>
```

### Test-Attribute-First Strategy Result:
```html
<!-- Input -->  
<button>Save</button>

<!-- Output -->
<button data-testid="save-button">Save</button>
```

## License

This project follows standard open-source licensing practices.
