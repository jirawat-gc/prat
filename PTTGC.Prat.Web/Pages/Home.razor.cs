using Microsoft.AspNetCore.Components;
using PTTGC.Prat.Core;

namespace PTTGC.Prat.Web.Pages;

public partial class Home : ComponentBase
{
    /// <summary>
    /// Current workspace session
    /// </summary>
    public Workspace Workspace { get; set; } = new();
    
    public MaterialAttribute NewMaterialAttributeItem = new();

    public List<(string attribute, string unit)> _CommonAttributes = new()
    {
        new ("Melt Flow Rate", "g/10 min"),
        new ("Density", "g/cm³"),
        new ("Vicat softening point", "℃"),
        new ("Melting Temperature", "℃"),
        new ("Tensile Strength at Yield", "kg/cm²"),
        new ("Tensile Strength at Break", "kg/cm²"),
        new ("Elongation at Break", "%"),
        new ("Stiffness", "kg/cm²"),
        new ("Flexural Modulus", "kg/cm²"),
        new ("Notched Izod Impact Strength", "kg.cm/cm"),
        new ("Durometer Hardness", "Shore D"),
        new ("ESCR, F50 (Condition B, 10 % Igepal)", "hrs"),
        new ("Lambda", "㎭/s"),
        new ("molegular weight", "㏖"),
    };

    private void ChangeMaterialAttribute( MaterialAttribute item, (string attribute, string unit) attr )
    {
        item.AttributeName = attr.attribute;
        item.MinRange = 0;
        item.MaxRange = 0;
        item.MeasurementUnit = attr.unit;

        this.StateHasChanged();
    }

    private bool IsCustomAttribute( MaterialAttribute item )
    {
        return _CommonAttributes.Any(a => a.attribute == item.AttributeName);
    }

    private string FixFlagKey( string flagKey)
    {
        return new string(flagKey.ToLowerInvariant()
                    .Replace(" ", "_")
                    .Where( c => char.IsLetterOrDigit(c))
                    .ToArray() );
    }
}
