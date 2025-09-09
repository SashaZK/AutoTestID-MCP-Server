using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace AutoTestId.McpServer.Tools;

/// <summary>
/// Represents an interactive HTML element that can receive a data-testid attribute
/// Enhanced with ARIA analysis capabilities for enterprise-grade locator strategies
/// </summary>
public class InteractiveElement
{
    public int Position { get; set; }
    public string ElementType { get; set; } = string.Empty;
    public string InnerText { get; set; } = string.Empty;
    public string Attributes { get; set; } = string.Empty;
    public string SuggestedTestId { get; set; } = string.Empty;
    public string FullElement { get; set; } = string.Empty;
    public bool HasExistingTestId { get; set; }
    
    // ARIA Analysis Properties
    public bool HasAriaLabel { get; set; }
    public bool HasAriaRole { get; set; }
    public string AriaLabel { get; set; } = string.Empty;
    public string AriaRole { get; set; } = string.Empty;
    public bool NeedsTestId { get; set; } = true;
    public string StrategyReason { get; set; } = string.Empty;
    public string SuggestedAriaLabel { get; set; } = string.Empty;
    public string SuggestedAriaRole { get; set; } = string.Empty;
}

/// <summary>
/// Tools for adding data-testid attributes to HTML elements for automated testing
/// </summary>
public static class TestIdTools
{
    /// <summary>
    /// Implements comprehensive AutoTestID workflow with two-phase processing aligned with prompt requirements:
    /// Phase 1: Strategy selection (aria-first vs test-attribute-first)  
    /// Phase 2: Strategy implementation with validation and user confirmation
    /// Returns formatted analysis results matching prompt specification exactly
    /// </summary>
    /// <param name="logger">Logger instance for diagnostic information</param>
    /// <param name="htmlContent">The HTML content to analyze and process</param>
    /// <returns>Two-phase workflow prompt for strategy selection and implementation</returns>
    public static string AddTestIDFromPrompt(
        ILogger logger,
        string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            return "Please provide HTML content to add locator attributes.";
        }

        try
        {
            // Load the comprehensive prompt template content
            string promptContent = GetTestIdPromptContent(logger);

            if (!string.IsNullOrEmpty(promptContent))
            {
                // Return the two-phase workflow prompt with the HTML content
                return $"AUTOTESTID WORKFLOW - Two-Phase Processing\n\n" +
                       $"HTML Content to Process: {htmlContent}\n\n" +
                       $"{promptContent}";
            }
            else
            {
                // Fallback to strategy selection if prompt file cannot be found
                return HandleStrategySelection(logger, htmlContent);
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error loading comprehensive AutoTestID prompt content");
            return $"An error occurred while loading the AutoTestID workflow: {ex.Message}. Please try again later.";
        }
    }

    /// <summary>
    /// Handles Phase 1: Strategy Selection for AutoTestID workflow
    /// Prompts user to choose between aria-first and test-attribute-first approaches
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="htmlContent">HTML content to process</param>
    /// <returns>Strategy selection prompt</returns>
    public static string HandleStrategySelection(ILogger logger, string htmlContent)
    {
        return @"
🎯 **AutoTestID Workflow - Phase 1: Strategy Selection**

Before analyzing or modifying HTML code, please choose your locator strategy:

**Option 1: Type `aria-first`**
- Prioritize ARIA roles and labels for accessibility-first approach
- Add `data-testid` only when ARIA attributes are insufficient
- Best for user-centric testing and accessibility compliance

**Option 2: Type `test-attribute-first`** 
- Add `data-testid` to all interactive elements for explicit test targeting
- Consistent coverage regardless of ARIA presence
- Best for comprehensive test automation coverage

**HTML Content to Process:**
" + htmlContent + @"

**Please respond with either `aria-first` or `test-attribute-first` to proceed to Phase 2.**
";
    }

    /// <summary>
    /// Processes HTML with ARIA-first strategy following updated prompt requirements
    /// Phase 2A: ARIA-First Implementation - adds ARIA OR data-testid, never both
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="htmlContent">HTML content to process</param>
    /// <returns>ARIA-first strategy results in exact prompt format</returns>
    public static string ProcessAriaFirstStrategy(ILogger logger, string htmlContent)
    {
        var elements = AnalyzeInteractiveElementsWithAria(logger, htmlContent);
        var ariaElements = elements.Where(e => e.HasAriaLabel || e.HasAriaRole).ToList();
        var needsTestId = elements.Where(e => e.NeedsTestId).ToList();
        var needsAriaOnly = elements.Where(e => !e.HasAriaLabel && !e.HasAriaRole && !e.NeedsTestId).ToList();

        var result = $@"🎯 **ARIA-First Strategy Results**

**Elements with sufficient ARIA/semantic targeting:** {ariaElements.Count}
{string.Join("\n", ariaElements.Select(e => $"• {e.ElementType} - {e.StrategyReason}"))}

**Elements needing data-testid:** {needsTestId.Count}
{string.Join("\n", needsTestId.Select(e => $"• {e.ElementType} → {e.SuggestedTestId}"))}

**ARIA attributes added:** {needsAriaOnly.Count}
{string.Join("\n", needsAriaOnly.Select(e => $"• {e.ElementType} → {e.SuggestedAriaLabel}"))}

**Recommendation:** ARIA-first approach - add ARIA attributes OR data-testid, never both to same element
**Next Step:** Review and confirm the ARIA enhancements OR selective test ID additions";

        return result;
    }

    /// <summary>
    /// Processes HTML with test-attribute-first strategy for comprehensive coverage
    /// Phase 2B: Test-Attribute-First Implementation with exact prompt format compliance
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="htmlContent">HTML content to process</param>
    /// <returns>Test-attribute-first strategy results in exact prompt format</returns>
    public static string ProcessTestAttributeFirstStrategy(ILogger logger, string htmlContent)
    {
        var elements = AnalyzeInteractiveElements(logger, htmlContent);
        var withTestIds = elements.Where(e => e.HasExistingTestId).ToList();
        var needsTestIds = elements.Where(e => !e.HasExistingTestId).ToList();

        var result = $@"🎯 **Test-Attribute-First Strategy Results**

**Elements with existing data-testid:** {withTestIds.Count}
{string.Join("\n", withTestIds.Select(e => $"• {e.ElementType} - preserved existing test ID"))}

**Elements needing data-testid:** {needsTestIds.Count}
{string.Join("\n", needsTestIds.Select(e => $"• {e.ElementType} → {e.SuggestedTestId}"))}

**Total Coverage:** {elements.Count} interactive elements identified
**Naming Convention:** kebab-case with context and element type

**Recommendation:** Comprehensive test coverage with explicit locator attributes for all interactive elements
**Next Step:** Review and confirm the complete test ID additions";

        return result;
    }

    /// <summary>
    /// Enhanced analysis that includes ARIA attribute detection and strategy reasoning
    /// Supports both aria-first and test-attribute-first approaches
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="htmlContent">HTML content to analyze</param>
    /// <returns>List of interactive elements with ARIA analysis</returns>
    public static List<InteractiveElement> AnalyzeInteractiveElementsWithAria(ILogger logger, string htmlContent)
    {
        var elements = AnalyzeInteractiveElements(logger, htmlContent);
        
        foreach (var element in elements)
        {
            // Analyze ARIA attributes
            element.HasAriaLabel = Regex.IsMatch(element.Attributes, @"aria-label\s*=", RegexOptions.IgnoreCase);
            element.HasAriaRole = Regex.IsMatch(element.Attributes, @"role\s*=", RegexOptions.IgnoreCase);
            
            if (element.HasAriaLabel)
            {
                var match = Regex.Match(element.Attributes, @"aria-label\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase);
                element.AriaLabel = match.Success ? match.Groups[1].Value : string.Empty;
            }
            
            if (element.HasAriaRole)
            {
                var match = Regex.Match(element.Attributes, @"role\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase);
                element.AriaRole = match.Success ? match.Groups[1].Value : string.Empty;
            }
            
            // Determine if element needs test ID based on updated ARIA-first strategy
            // Key Rule: Do not add ARIA attributes and data-testid to the same element
            // Priority: ARIA attributes first, data-testid only when ARIA is insufficient
            
            if (element.HasAriaLabel && !string.IsNullOrWhiteSpace(element.AriaLabel))
            {
                element.NeedsTestId = false;
                element.StrategyReason = $"Sufficient ARIA label: '{element.AriaLabel}' - no test ID needed";
            }
            else if (element.HasAriaRole && !string.IsNullOrWhiteSpace(element.AriaRole) && !string.IsNullOrWhiteSpace(element.InnerText))
            {
                element.NeedsTestId = false;
                element.StrategyReason = $"ARIA role '{element.AriaRole}' with meaningful text content - no test ID needed";
            }
            else if (element.HasAriaRole && !string.IsNullOrWhiteSpace(element.AriaRole))
            {
                element.NeedsTestId = false;
                element.StrategyReason = $"ARIA role '{element.AriaRole}' sufficient - no test ID needed";
            }
            else if (!string.IsNullOrWhiteSpace(element.InnerText) && element.InnerText.Length > 2)
            {
                element.NeedsTestId = false;
                element.StrategyReason = $"Will add ARIA attributes for accessibility - no test ID needed";
                
                // Suggest ARIA improvements instead of test IDs
                element.SuggestedAriaLabel = GenerateAriaLabel(element);
                element.SuggestedAriaRole = GenerateAriaRole(element);
            }
            else
            {
                // Only add data-testid when ARIA attributes are insufficient for reliable targeting
                element.NeedsTestId = true;
                element.StrategyReason = "ARIA attributes insufficient for reliable targeting - needs test ID";
                
                // For elements that truly need test IDs, don't add ARIA to avoid duplication
                element.SuggestedAriaLabel = string.Empty;
                element.SuggestedAriaRole = string.Empty;
            }
        }
        
        logger?.LogInformation($"ARIA analysis complete: {elements.Count(e => e.HasAriaLabel || e.HasAriaRole)} elements with ARIA, {elements.Count(e => e.NeedsTestId)} need test IDs");
        return elements;
    }

    /// <summary>
    /// Processes HTML content with the selected strategy and returns comprehensive results
    /// Phase 2: Strategy Implementation with validation and user confirmation
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="htmlContent">HTML content to process</param>
    /// <param name="strategy">Selected strategy: "aria-first" or "test-attribute-first"</param>
    /// <returns>Strategy-specific processing results with recommendations</returns>
    public static string ProcessHtmlWithStrategy(ILogger logger, string htmlContent, string strategy)
    {
        if (string.IsNullOrWhiteSpace(strategy))
        {
            return HandleStrategySelection(logger, htmlContent);
        }

        return strategy.ToLower().Trim() switch
        {
            "aria-first" => ProcessAriaFirstStrategy(logger, htmlContent),
            "test-attribute-first" => ProcessTestAttributeFirstStrategy(logger, htmlContent),
            _ => $"Invalid strategy '{strategy}'. Please choose 'aria-first' or 'test-attribute-first'.\n\n" + 
                 HandleStrategySelection(logger, htmlContent)
        };
    }

    /// <summary>
    /// Generates code preview with diff-style formatting as required by prompt
    /// Shows before/after comparison for modified HTML elements
    /// </summary>
    /// <param name="elements">List of elements to preview</param>
    /// <param name="strategy">Selected strategy for appropriate formatting</param>
    /// <returns>Formatted code preview with highlighted changes</returns>
    public static string GenerateCodePreview(List<InteractiveElement> elements, string strategy)
    {
        if (!elements.Any()) return "No elements to preview.";

        var preview = "### Code Preview:\n\n";
        
        foreach (var element in elements.Take(5)) // Limit preview to first 5 elements
        {
            preview += $"**{element.ElementType}:**\n";
            preview += "```html\n";
            
            if (strategy == "aria-first")
            {
                preview += $"// BEFORE:\n{element.FullElement}\n\n";
                
                // Show ARIA OR test ID additions (never both per updated prompt)
                if (!string.IsNullOrEmpty(element.SuggestedAriaLabel) && !element.NeedsTestId)
                {
                    preview += $"// AFTER (with ARIA only):\n";
                    preview += element.FullElement.Replace(">", $" aria-label=\"{element.SuggestedAriaLabel}\" role=\"{element.SuggestedAriaRole}\">");
                    preview += "\n";
                }
                else if (element.NeedsTestId)
                {
                    preview += $"// AFTER (with test ID only):\n";
                    preview += element.FullElement.Replace(">", $" data-testid=\"{element.SuggestedTestId}\">");
                    preview += "\n";
                }
                else
                {
                    preview += $"// NO CHANGES NEEDED - sufficient ARIA/semantic targeting\n";
                }
            }
            else if (strategy == "test-attribute-first")
            {
                preview += $"// BEFORE:\n{element.FullElement}\n\n";
                preview += $"// AFTER (with test ID only):\n";
                preview += element.FullElement.Replace(">", $" data-testid=\"{element.SuggestedTestId}\">");
                preview += "\n";
            }
            
            preview += "```\n\n";
        }
        
        if (elements.Count > 5)
        {
            preview += $"... and {elements.Count - 5} more elements\n";
        }
        
        return preview;
    }

    /// <summary>
    /// Analyzes HTML content and returns a list of interactive elements that can receive test IDs
    /// </summary>
    /// <param name="logger">Logger instance for diagnostic information</param>
    /// <param name="htmlContent">The HTML content to analyze</param>
    /// <returns>List of interactive elements found in the HTML</returns>
    public static List<InteractiveElement> AnalyzeInteractiveElements(ILogger logger, string htmlContent)
    {
        var elements = new List<InteractiveElement>();
        
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            logger?.LogWarning("No HTML content provided for analysis");
            return elements;
        }

        try
        {
            // Interactive element patterns with their suggested naming
            var patterns = new[]
            {
                new { Pattern = @"<button[^>]*>(.*?)</button>", Type = "button", Suffix = "button" },
                new { Pattern = @"<input[^>]*type=[""']?text[""']?[^>]*>", Type = "text input", Suffix = "input" },
                new { Pattern = @"<input[^>]*type=[""']?password[""']?[^>]*>", Type = "password input", Suffix = "input" },
                new { Pattern = @"<input[^>]*type=[""']?email[""']?[^>]*>", Type = "email input", Suffix = "input" },
                new { Pattern = @"<input[^>]*type=[""']?checkbox[""']?[^>]*>", Type = "checkbox", Suffix = "checkbox" },
                new { Pattern = @"<input[^>]*type=[""']?radio[""']?[^>]*>", Type = "radio button", Suffix = "radio" },
                new { Pattern = @"<input[^>]*type=[""']?submit[""']?[^>]*>", Type = "submit button", Suffix = "button" },
                new { Pattern = @"<select[^>]*>.*?</select>", Type = "select", Suffix = "select" },
                new { Pattern = @"<textarea[^>]*>.*?</textarea>", Type = "textarea", Suffix = "textarea" },
                new { Pattern = @"<a[^>]*href=[^>]*>(.*?)</a>", Type = "link", Suffix = "link" }
            };

            int position = 1;
            
            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(htmlContent, pattern.Pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                
                foreach (Match match in matches)
                {
                    var element = new InteractiveElement
                    {
                        Position = position++,
                        ElementType = pattern.Type,
                        FullElement = match.Value,
                        Attributes = ExtractAttributes(match.Value),
                        InnerText = ExtractInnerText(match.Value, pattern.Type),
                        HasExistingTestId = match.Value.Contains("data-testid", StringComparison.OrdinalIgnoreCase)
                    };
                    
                    element.SuggestedTestId = GenerateSuggestedTestId(element, pattern.Suffix);
                    elements.Add(element);
                }
            }

            logger?.LogInformation($"Found {elements.Count} interactive elements in HTML");
            return elements.OrderBy(e => htmlContent.IndexOf(e.FullElement)).Select((e, i) => { e.Position = i + 1; return e; }).ToList();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error analyzing interactive elements");
            return elements;
        }
    }

    private static string ExtractAttributes(string element)
    {
        var match = Regex.Match(element, @"<\w+\s+([^>]*?)/?>");
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }

    private static string ExtractInnerText(string element, string elementType)
    {
        if (elementType.Contains("input") || elementType.Contains("checkbox") || elementType.Contains("radio"))
        {
            // For inputs, try to get placeholder or value
            var placeholder = Regex.Match(element, @"placeholder=[""']([^""']*)[""']", RegexOptions.IgnoreCase);
            if (placeholder.Success) return placeholder.Groups[1].Value;
            
            var value = Regex.Match(element, @"value=[""']([^""']*)[""']", RegexOptions.IgnoreCase);
            if (value.Success) return value.Groups[1].Value;
            
            return string.Empty;
        }
        
        // For other elements, get inner text
        var innerText = Regex.Match(element, @">([^<]*)<", RegexOptions.Singleline);
        return innerText.Success ? innerText.Groups[1].Value.Trim() : string.Empty;
    }

    private static string GenerateSuggestedTestId(InteractiveElement element, string suffix)
    {
        var baseName = string.Empty;
        
        // Try to derive name from inner text first
        if (!string.IsNullOrWhiteSpace(element.InnerText))
        {
            baseName = element.InnerText.ToLower()
                .Replace(" ", "-")
                .Replace("_", "-")
                .Trim();
        }
        
        // Fallback to attributes
        if (string.IsNullOrWhiteSpace(baseName))
        {
            var nameMatch = Regex.Match(element.Attributes, @"name=[""']([^""']*)[""']", RegexOptions.IgnoreCase);
            if (nameMatch.Success)
            {
                baseName = nameMatch.Groups[1].Value.ToLower().Replace("_", "-");
            }
        }
        
        // Fallback to id attribute
        if (string.IsNullOrWhiteSpace(baseName))
        {
            var idMatch = Regex.Match(element.Attributes, @"id=[""']([^""']*)[""']", RegexOptions.IgnoreCase);
            if (idMatch.Success)
            {
                baseName = idMatch.Groups[1].Value.ToLower().Replace("_", "-");
            }
        }
        
        // Final fallback
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = element.ElementType.Replace(" ", "-");
        }
        
        // Clean up the name
        baseName = Regex.Replace(baseName, @"[^a-z0-9\-]", "", RegexOptions.IgnoreCase);
        baseName = Regex.Replace(baseName, @"-+", "-").Trim('-');
        
        return string.IsNullOrWhiteSpace(baseName) ? suffix : $"{baseName}-{suffix}";
    }

    /// <summary>
    /// Generates appropriate ARIA label suggestions for elements lacking accessibility attributes
    /// </summary>
    /// <param name="element">Interactive element to generate ARIA label for</param>
    /// <returns>Suggested ARIA label text</returns>
    private static string GenerateAriaLabel(InteractiveElement element)
    {
        if (!string.IsNullOrWhiteSpace(element.InnerText))
        {
            return $"{element.InnerText.Trim()} {element.ElementType}";
        }
        
        // Extract meaningful text from attributes
        var placeholder = Regex.Match(element.Attributes, @"placeholder=[""']([^""']*)[""']", RegexOptions.IgnoreCase);
        if (placeholder.Success)
        {
            return $"Enter {placeholder.Groups[1].Value.ToLower()}";
        }
        
        var value = Regex.Match(element.Attributes, @"value=[""']([^""']*)[""']", RegexOptions.IgnoreCase);
        if (value.Success)
        {
            return $"{value.Groups[1].Value} {element.ElementType}";
        }
        
        var name = Regex.Match(element.Attributes, @"name=[""']([^""']*)[""']", RegexOptions.IgnoreCase);
        if (name.Success)
        {
            return $"{name.Groups[1].Value.Replace("_", " ")} {element.ElementType}";
        }
        
        return $"{element.ElementType.Replace("-", " ")}";
    }

    /// <summary>
    /// Generates appropriate ARIA role suggestions for elements based on their type and context
    /// </summary>
    /// <param name="element">Interactive element to generate ARIA role for</param>
    /// <returns>Suggested ARIA role</returns>
    private static string GenerateAriaRole(InteractiveElement element)
    {
        return element.ElementType.ToLower() switch
        {
            "button" or "submit button" => "button",
            "text input" or "password input" or "email input" => "textbox",
            "checkbox" => "checkbox",
            "radio button" => "radio",
            "select" => "combobox",
            "textarea" => "textbox",
            "link" => "link",
            _ => "button" // Default fallback for interactive elements
        };
    }

    private static string GetTestIdPromptContent(ILogger? logger)
    {
        try
        {
            // Possible locations to look for the prompt file
            var possiblePaths = new[]
            {
                // Current application directory - tech_prompts
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tech_prompts", "add_test_id_prompt.md"),
                // One level up from application directory - tech_prompts
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "tech_prompts", "add_test_id_prompt.md"),
                // Current working directory - tech_prompts
                Path.Combine(Directory.GetCurrentDirectory(), "tech_prompts", "add_test_id_prompt.md"),
                // Relative to project root - tech_prompts
                Path.Combine(Directory.GetCurrentDirectory(), "AutoTestId.McpServer", "tech_prompts", "add_test_id_prompt.md"),
                // Package root (for distributed installations)
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "add_test_id_prompt.md"),
                // Current working directory root
                Path.Combine(Directory.GetCurrentDirectory(), "add_test_id_prompt.md")
            };

            // Log diagnostic information
            logger?.LogInformation($"Current working directory: {Directory.GetCurrentDirectory()}");
            logger?.LogInformation($"Application base directory: {AppDomain.CurrentDomain.BaseDirectory}");

            // Check each possible path
            foreach (var path in possiblePaths)
            {
                logger?.LogInformation($"Checking path: {path}");
                if (File.Exists(path))
                {
                    logger?.LogInformation($"Found test ID prompt at: {path}");
                    return File.ReadAllText(path);
                }
            }

            logger?.LogWarning("Could not find test ID generator prompt file in any of the expected locations.");
            return string.Empty;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error searching for test ID prompt file");
            return string.Empty;
        }
    }
}
