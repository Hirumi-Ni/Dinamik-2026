
using UnityEngine;
using UnityEngine.UI;

public class FlexibleGridLayout : LayoutGroup
{
    public enum FitType
    {
        Uniform,
        Width,
        Height,
        FixedRows,
        FixedColumns
    }

    public FitType fitType;
    public int rows;
    public int columns;
    public Vector2 cellSize;
    public Vector2 spacing;
    public bool fitX;
    public bool fitY;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        if (fitType == FitType.Width ||
            fitType == FitType.Height ||
            fitType == FitType.Uniform)
        {
            float sqrt = Mathf.Sqrt(rectChildren.Count);
            rows = Mathf.CeilToInt(sqrt);
            columns = Mathf.CeilToInt(sqrt);
        }

        if (fitType == FitType.Width ||
            fitType == FitType.FixedColumns)
        {
            rows = Mathf.CeilToInt(
                rectChildren.Count / (float)columns);
        }

        if (fitType == FitType.Height ||
            fitType == FitType.FixedRows)
        {
            columns = Mathf.CeilToInt(
                rectChildren.Count / (float)rows);
        }

        rows = Mathf.Max(1, rows);
        columns = Mathf.Max(1, columns);

        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;

        float cellWidth = (parentWidth / columns)
            - (spacing.x * (columns - 1) / columns)
            - ((padding.left + padding.right) / (float)columns);

        float cellHeight = (parentHeight / rows)
            - (spacing.y * (rows - 1) / rows)
            - ((padding.top + padding.bottom) / (float)rows);

        if (fitX)
            cellSize.x = cellWidth;

        if (fitY)
            cellSize.y = cellHeight;
    }

    public override void CalculateLayoutInputVertical()
    {
        float minHeight = padding.vertical +
            rows * cellSize.y +
            Mathf.Max(0, rows - 1) * spacing.y;

        SetLayoutInputForAxis(
            minHeight,
            minHeight,
            -1,
            1
        );
    }

    public override void SetLayoutHorizontal()
    {
        SetLayout(0);
    }

    public override void SetLayoutVertical()
    {
        SetLayout(1);
    }

    private void SetLayout(int axis)
    {
        for (int i = 0; i < rectChildren.Count; i++)
        {
            int rowCount = i / columns;
            int columnCount = i % columns;

            float xPos = cellSize.x * columnCount
                + spacing.x * columnCount
                + padding.left;

            float yPos = cellSize.y * rowCount
                + spacing.y * rowCount
                + padding.top;

            if (axis == 0)
            {
                SetChildAlongAxis(
                    rectChildren[i],
                    0,
                    xPos,
                    cellSize.x
                );
            }
            else
            {
                SetChildAlongAxis(
                    rectChildren[i],
                    1,
                    yPos,
                    cellSize.y
                );
            }
        }
    }
}
