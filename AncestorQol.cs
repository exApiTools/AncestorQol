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
            for (var optionIndex = 0; optionIndex < options.Count; optionIndex++)
            {
                var option = options[optionIndex];
                try
                {
                    var tribeName = option.GetChildFromIndices(1, 0, 0)?.Text;
                    var rewardTier = string.IsNullOrEmpty(tribeName) ? -1 : Settings.GetTribeRewardTier(tribeName);
                    var rewardColor = TierToColor(rewardTier);
                    var rewardRect = (Settings.DrawTribeRewardFrameOverTheWholeElement?option: option[2])?.GetClientRectCache ?? default;
                    rewardRect.Inflate(0, -Settings.FrameThickness);
                    if (rewardRect.Intersects(containerRect))
                    {
                        Graphics.DrawFrame(rewardRect, rewardColor, Settings.FrameThickness);
                    }

                    var tribeFavourReward = option.GetChildFromIndices(3, 0)?.Children ?? new List<Element>();
                    foreach (var tribe in tribeFavourReward)
                    {
                        var favorTribeName = tribe[0]?.Tooltip?.Text;
                        var tier = Settings.GetTribeShopTier(favorTribeName);
                        var color = TierToColor(tier);
                        var rect = tribe.GetClientRectCache;
                        rect.Inflate(-5, 0);
                        if (!rect.Intersects(containerRect)) continue;
                        Graphics.DrawFrame(rect, color, Settings.FrameThickness);

                        var favorNote = Settings.FavorNotes.GetValueOrDefault(favorTribeName ?? string.Empty);
                        if (!string.IsNullOrEmpty(favorNote))
                        {
                            var textSize = Graphics.MeasureText(favorNote);
                            var textOffset = new Vector2((rect.Width - textSize.X) / 2, 0);
                            var textPos = rect.BottomLeft.ToVector2Num() + textOffset;
                            Graphics.DrawBox(rect.BottomLeft.ToVector2Num(), rect.BottomRight.ToVector2Num() + new Vector2(0, textSize.Y), Color.Black);
                            Graphics.DrawText(favorNote, textPos, Color.White);
                        }
                    }

                    var favorContainerRect = option[3]?.GetClientRectCache ?? default;
                    var rewardsByTier = GameController.IngameState.ServerData.AncestorFights.Options.ElementAtOrDefault(optionIndex)?.Rewards
                        .ToLookup(x => Settings.GetTribeShopTier(x.FavorTribe.NameTribe));
                    if (rewardsByTier != null)
                    {
                        var colors = new Color[] { Settings.Tier1Color, Settings.Tier2Color, Settings.Tier3Color, Settings.Tier4Color, Settings.Tier5Color };

                        var lines = new List<(string Text, Color Color)>();
                        for (int i = 1; i <= 5; i++)
                        {
                            var sum = rewardsByTier[i].Sum(x => x.FavorAmount);
                            if (sum != 0)
                            {
                                lines.Add(($"{$"T{i}",5}: {sum,5}", colors[i - 1]));
                            }
                        }

                        if (lines.Count > 0)
                        {
                            var completeSum = rewardsByTier.SelectMany(x => x).Sum(x => x.FavorAmount);
                            // Only worth a separate total line when more than one tier contributes.
                            if (lines.Count > 1)
                            {
                                lines.Add(($"{"Total",5}: {completeSum,5}", Color.White));
                            }

                            var lineHeight = Graphics.MeasureText(lines[0].Text).Y;
                            var blockWidth = lines.Max(l => Graphics.MeasureText(l.Text).X);
                            var blockHeight = lineHeight * lines.Count;
                            const float padX = 3;
                            var blockPos = new Vector2(favorContainerRect.Center.X - blockWidth / 2, favorContainerRect.Top);

                            if (containerRect.Contains(blockPos))
                            {
                                Graphics.DrawBox(
                                    blockPos - new Vector2(padX, 0),
                                    blockPos + new Vector2(blockWidth + padX, blockHeight),
                                    Color.Black);

                                var linePos = blockPos;
                                foreach (var (text, color) in lines)
                                {
                                    // Thin divider above the summed total to set it apart from the tiers.
                                    if (lines.Count > 1 && text.StartsWith("Total"))
                                    {
                                        Graphics.DrawLine(
                                            linePos - new Vector2(padX, 0),
                                            linePos + new Vector2(blockWidth + padX, 0),
                                            1, Color.Gray);
                                    }

                                    Graphics.DrawText(text, linePos, color);
                                    linePos.Y += lineHeight;
                                }
                            }
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
                    Graphics.DrawFrame(optionRect, color, Settings.FrameThickness);
                    Graphics.DrawText(tooltipDescription, topRight + new Vector2(textPadding), Color.White);
                    if (!string.IsNullOrEmpty(unitNote))
                    {
                        var unitNoteRect = option[2]?.GetClientRectCache ?? default;
                        Graphics.DrawBox(unitNoteRect, Color.Black);
                        Graphics.DrawText(unitNote, unitNoteRect.TopLeft.ToVector2Num());
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
                        var unitNoteRect = option[2]?.GetClientRectCache ?? default;
                        Graphics.DrawBox(unitNoteRect, Color.Black);
                        Graphics.DrawText(unitNote, unitNoteRect.TopLeft.ToVector2Num());
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
