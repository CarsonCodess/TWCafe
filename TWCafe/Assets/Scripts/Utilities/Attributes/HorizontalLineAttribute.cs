using UnityEngine;

public class HorizontalLineAttribute : PropertyAttribute
{
    public int Thickness { get; set; } = 1;
    public float Padding { get; set; } = 10f;
    
    public HorizontalLineAttribute() { }
}
