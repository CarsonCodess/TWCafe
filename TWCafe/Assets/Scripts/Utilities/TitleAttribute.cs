using UnityEngine;

public class TitleAttribute : PropertyAttribute
{
    public string Title { get; }

    public TitleAttribute(string title)
    {
        Title = title;
    }
}
