using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

public class BuildTableService
{
    public RenderFragment BuildTableFragment<T>(
        IEnumerable<T> items,
        string[] headers,
        Func<T, object[]> valueSelector,
        NavigationManager navigationManager,
        Func<T, string> navigateUrlSelector = null,
        Action<string> onHeaderClick = null)
    {
        return builder =>
        {
            if (items != null && items.Any())
            {
                int seq = 0;
                builder.OpenElement(seq++, "table");
                builder.AddAttribute(seq++, "class", "table");

                BuildTableHeaderOrFooter(builder, ref seq, headers, onHeaderClick, true);
                BuildTableBody(builder, ref seq, items, headers, valueSelector, navigateUrlSelector, navigationManager);
                BuildTableHeaderOrFooter(builder, ref seq, headers, onHeaderClick, false);

                builder.CloseElement(); // Close table
            }
        };
    }

    private void BuildTableHeaderOrFooter(RenderTreeBuilder builder, ref int seq, string[] headers, Action<string> onHeaderClick, bool isHeader)
    {
        var tag = isHeader ? "thead" : "tfoot";
        builder.OpenElement(seq++, tag);
        BuildTableRow(builder, ref seq, headers, onHeaderClick);
        builder.CloseElement(); // Close tag (thead or tfoot)
    }

    private void BuildTableRow(RenderTreeBuilder builder, ref int seq, string[] headers, Action<string> onHeaderClick)
    {
        builder.OpenElement(seq++, "tr");
        foreach (var header in headers)
        {
            builder.OpenElement(seq++, "th");
            if (onHeaderClick != null)
            {
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => onHeaderClick(header)));
            }
            builder.AddContent(seq++, header);
            builder.CloseElement(); // Close th
        }
        builder.CloseElement(); // Close tr
    }
    private void BuildTableBody<T>(
       RenderTreeBuilder builder,
       ref int seq,
       IEnumerable<T> items,
       string[] headers,
       Func<T, object[]> valueSelector,
       Func<T, string> navigateUrlSelector,
       NavigationManager navigationManager)
    {
        builder.OpenElement(seq++, "tbody");
        foreach (var item in items)
        {
            builder.OpenElement(seq++, "tr");

            var values = valueSelector(item);
            for (int i = 0; i < values.Length; i++)
            {
                var value = values[i];
                bool isCompanyName = headers[i] == "CompanyName";

                if (isCompanyName && navigateUrlSelector != null)
                {
                    string navigateUrl = navigateUrlSelector(item);
                    builder.OpenElement(seq++, "td");
                    builder.AddAttribute(seq++, "class", "clickable-cell");
                    builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => navigationManager.NavigateTo(navigateUrl)));
                }
                else
                {
                    builder.OpenElement(seq++, "td");
                }

                builder.AddContent(seq++, value?.ToString() ?? "N/A");
                builder.CloseElement(); // Close td
            }

            builder.CloseElement(); // Close tr
        }
        builder.CloseElement(); // Close tbody
    }
}
