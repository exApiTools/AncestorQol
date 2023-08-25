using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.Shared.Helpers;
using SharpDX;
using Vector2 = System.Numerics.Vector2;

namespace AncestorQol;

public class AncestorQol : BaseSettingsPlugin<AncestorQolSettings>
{
    private static readonly Regex CleanDescriptionRegex = new Regex("<[^>]*>{(?<value>[^}]*)}", RegexOptions.Compiled);
    private static readonly Regex LineEndingRegex = new Regex("(\r?\n){3,}", RegexOptions.Compiled);
    private static readonly Regex SentenceSplitRegex = new Regex(@"\.( +)", RegexOptions.Compiled);

    public override void Render()
    {
        RenderUnitTier();
        RenderTribeTier();
    }

    public void RenderTribeTier()
    {
        if (GameController.IngameState.IngameUi.AncestorFightSelectionWindow is
            {
                IsVisible: true,
                Options: { Count: > 0 } options,
                TableContainer.GetClientRectCache: var containerRect
            })
        {
            foreach (var option in options)
            {
                try
                {
                    var tribeName = option.GetChildFromIndices(2, 0, 0)?.Text;
                    var rewardTier = string.IsNullOrEmpty(tribeName) ? -1 : Settings.GetTribeRewardTier(tribeName);
                    var rewardColor = TierToColor(rewardTier);
                    var rewardRect = option[2].GetClientRectCache;
                    if (rewardRect.Intersects(containerRect))
                    {
                        Graphics.DrawFrame(rewardRect, rewardColor, Settings.FrameThickness);
                    }

                    var tribeFavourReward = option.GetChildFromIndices(3, 0)?.Children ?? new List<Element>();
                    foreach (var tribe in tribeFavourReward)
                    {
                        var tooltip = tribe[0]?.Tooltip?.Text;
                        var tier = Settings.GetTribeShopTier(tooltip);
                        var favorNote = Settings.FavorNotes.GetValueOrDefault(tooltip ?? string.Empty);
                        var color = TierToColor(tier);
                        var rect = tribe.GetClientRectCache;
                        rect.Inflate(-5, 0);
                        if (!rect.Intersects(containerRect)) continue;
                        Graphics.DrawFrame(rect, color, Settings.FrameThickness);

                        if (!string.IsNullOrEmpty(favorNote))
                        {
                            var favorNoteRect = rect;
                            favorNoteRect.Height = 30; // Adjust the height of the box as needed
                            favorNoteRect.X = rect.Left; // Place it at the left of the tribe rectangle
                            favorNoteRect.Y = rect.Bottom - favorNoteRect.Height; // Place it at the bottom

                            var boxVerticalOffset = -30; // You can adjust this value as needed
                            favorNoteRect.Y -= boxVerticalOffset;

                            //The above works for 4k, but for other resolutions needs to be converted to relative screen size units, or a ui element
                            //needs to be added to adjust these on a fixed slider if they are gonna be a fixed value.

                            Graphics.DrawBox(favorNoteRect, Color.Black);

                            // Calculate text position to center it within the black box
                            var textSize = Graphics.MeasureText(favorNote);
                            var textPosition = new Vector2(
                                favorNoteRect.Left + (favorNoteRect.Width - textSize.X) / 2,
                                favorNoteRect.Top + (favorNoteRect.Height - textSize.Y) / 2);

                            Graphics.DrawText(favorNote, textPosition, Color.White);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex.ToString());
                }
            }
        }
    }

    public void RenderUnitTier()
    {
        if (GameController.Game.IngameState.UIHover?.Tooltip is not { IsVisible: true } &&
            GameController.IngameState.IngameUi.AncestorMainShopWindow is { IsVisible: true, Options: { Count: > 0 } options })
        {
            foreach (var option in options)
            {
                try
                {
                    var unit = option.Unit;
                    var item = option.Item;
                    var tier = Settings.GetUnitTier(unit?.Id ?? item?.Id ?? string.Empty);
                    var unitNote = Settings.UnitNotes.GetValueOrDefault(unit?.Id ?? item?.Id ?? string.Empty);
                    var color = TierToColor(tier);
                    var tooltipDescription = (unit, item) switch
                    {
                        (_, { }) => string.Join("\n", option.Tooltip?.GetChildFromIndices(0, 0)?.Children.Where(x => x.IsVisible).Select(x => x.TextNoTags) ?? new List<string>()),
                        _ => option.Tooltip?.GetChildFromIndices(0, 0)?.TextNoTags,
                    };
                    if (string.IsNullOrWhiteSpace(tooltipDescription))
                    {
                        tooltipDescription = CleanDescriptionRegex.Replace(unit?.Description ?? string.Empty, "$1");
                    }

                    tooltipDescription = SentenceSplitRegex.Replace(LineEndingRegex.Replace(tooltipDescription, "\n\n"), ".\n");

                    var optionRect = option.GetClientRectCache;
                    var textPadding = Settings.FrameThickness + 2;
                    var rect = new Vector2(string.IsNullOrWhiteSpace(tooltipDescription) ? 0 : Graphics.MeasureText(tooltipDescription).X + textPadding * 2, optionRect.Height);
                    var topRight = optionRect.TopRight.ToVector2Num();
                    Graphics.DrawBox(topRight, topRight + rect, Color.Black);
                    Graphics.DrawFrame(topRight, topRight + rect, color, Settings.FrameThickness);
                    Graphics.DrawText(tooltipDescription, topRight + new Vector2(textPadding), Color.White);
                    if (!string.IsNullOrEmpty(unitNote))
                    {
                        var noteRect = option[2]?.GetClientRectCache ?? default;
                        Graphics.DrawBox(noteRect, Color.Black);
                        Graphics.DrawText(unitNote, noteRect.TopLeft.ToVector2Num());
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex.ToString());
                }
            }
        }

        if (GameController.IngameState.IngameUi.AncestorLeftShopPanel is { IsVisible: true, Options: { Count: > 0 } leftOptions })
        {
            foreach (var option in leftOptions)
            {
                try
                {
                    var unit = option.Unit;
                    var item = option.Item;
                    var tier = Settings.GetUnitTier(unit?.Id ?? item?.Id ?? string.Empty);
                    var unitNote = Settings.UnitNotes.GetValueOrDefault(unit?.Id ?? item?.Id ?? string.Empty);
                    var color = TierToColor(tier);

                    var optionRect = option.GetClientRectCache;
                    Graphics.DrawFrame(optionRect, color, Settings.FrameThickness);
                    if (!string.IsNullOrEmpty(unitNote))
                    {
                        var noteRect = option[2]?.GetClientRectCache ?? default;
                        Graphics.DrawBox(noteRect, Color.Black);
                        Graphics.DrawText(unitNote, noteRect.TopLeft.ToVector2Num());
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex.ToString());
                }
            }
        }
    }

    private Color TierToColor(int tier)
    {
        return tier switch
        {
            1 => Settings.Tier1Color.Value,
            2 => Settings.Tier2Color.Value,
            3 => Settings.Tier3Color.Value,
            4 => Settings.Tier4Color.Value,
            5 => Settings.Tier5Color.Value,
            _ => Color.Pink
        };
    }
}