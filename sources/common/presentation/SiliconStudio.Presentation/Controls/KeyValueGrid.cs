﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Controls;

namespace SiliconStudio.Presentation.Controls
{
    /// <summary>
    /// This control represents a <see cref="Grid"/> with two columns, the first one representing keys and the second one representing
    /// values. <see cref="Grid.ColumnDefinitions"/> and <see cref="Grid.RowDefinitions"/> should not be modified for this control. Every 
    /// child content added in this control will either create a new row and be placed on its left column, or placed on the second column
    /// of the last row.
    /// </summary>
    /// <remarks>The column for the keys has an <see cref="GridUnitType.Auto"/> width.</remarks>
    /// <remarks>The column for the values has an <see cref="GridUnitType.Star"/> width.</remarks>
    public class KeyValueGrid : Grid
    {
        private bool gridParametersInvalidated;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueGrid"/> class.
        /// </summary>
        public KeyValueGrid()
        {
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.0, GridUnitType.Auto) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star) });
        }

        /// <summary>
        /// Recomputes rows and update children.
        /// </summary>
        private void InvalidateGridParameters()
        {
            RowDefinitionCollection rowCollection = RowDefinitions;
            UIElementCollection children = Children;

            // Determine how many rows we need
            int remainder;
            int neededRowCount = Math.DivRem(children.Count, 2, out remainder) + remainder;

            int currentRowCount = rowCollection.Count;
            int deltaRowCount = neededRowCount - currentRowCount;

            // Add/remove rows
            if (deltaRowCount > 0)
            {
                for (int i = 0; i < deltaRowCount; i++)
                    rowCollection.Add(new RowDefinition { Height = new GridLength(0.0, GridUnitType.Auto) });
            }
            else if (deltaRowCount < 0)
            {
                rowCollection.RemoveRange(currentRowCount + deltaRowCount, -deltaRowCount);
            }

            // Update Grid.Row and Grid.Column dependency properties on each child control
            int row = 0;
            int column = 0;
            foreach (UIElement element in children)
            {
                element.SetValue(ColumnProperty, column);
                element.SetValue(RowProperty, row);

                column++;
                if (column > 1)
                {
                    column = 0;
                    row++;
                }
            }
        }

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size constraint)
        {
            if (gridParametersInvalidated)
            {
                gridParametersInvalidated = false;
                InvalidateGridParameters();
            }

            return base.MeasureOverride(constraint);
        }

        /// <inheritdoc/>
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            gridParametersInvalidated = true;
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }
    }
}
