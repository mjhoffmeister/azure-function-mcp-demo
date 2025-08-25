---
description: 'Code style guidelines for C# files'
applyTo: '**/*.cs'
---

# C# Code Style

## Variable declaration

Don't use `var` on the left-hand side of assignments. Always use explicit types
to improve readability and maintainability. Use target-typed `new` expressions
for conciseness.

### Example

```csharp
// Bad
var customer = new Customer();
var order = new Order();

// Good
Customer customer = new();
Order order = new();
```

## Comments

Always use XML documentation comments to describe classes, structs, records,
methods, and properties.

### Examples

```csharp
/// <summary>
/// Gets the customer by ID.
/// </summary>
/// <param name="id">The customer ID.</param>
/// <returns>The customer.</returns>
public Customer GetCustomerById(int id)
{
    // Implementation
}
```

```csharp
/// <summary>
/// Represents a customer.
/// </summary>
public class Customer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Customer"/> class.
    /// </summary>
    /// <param name="name">The customer's name.</param>
    /// <param name="email">The customer's email.</param>
    private Customer(string name, string email)
    {
        // Implementation
    }
}
```