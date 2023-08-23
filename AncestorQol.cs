using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ExileCore;
using ExileCore.PoEMemory.MemoryObjects.Ancestor;
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
        if (GameController.IngameState.IngameUi.AncestorFightSelectionWindow is { IsVisible: true, Options: { Count: > 0 } options })
        {
            foreach (var option in options)
            {
                try
                {
                    var tribeFavourReward = option.Children[3].Children[0].Children;
                    foreach (var tribe in tribeFavourReward)
                    {
                        
                        var tooltip = tribe.Children[0].Tooltip.Text;
                        var tier = Settings.GetTribeTier(tooltip);
                        var color = tier switch
                        {
                            1 => Settings.Tier1Color.Value,
                            2 => Settings.Tier2Color.Value,
                            3 => Settings.Tier3Color.Value,
                            _ => Color.Pink
                        };
                        var rect = tribe.GetClientRectCache;
                        rect.Inflate(-5,0);
                        if (!rect.Intersects(GameController.IngameState.IngameUi.AncestorFightSelectionWindow.TableContainer.GetClientRectCache)) continue;
                        Graphics.DrawFrame(rect, color, Settings.FrameThickness);

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
                    var color = tier switch
                    {
                        1 => Settings.Tier1Color.Value,
                        2 => Settings.Tier2Color.Value,
                        3 => Settings.Tier3Color.Value,
                        _ => Color.Pink
                    };
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
                }
                catch (Exception ex)
                {
                    LogError(ex.ToString());
                }
            }
        }

    }
}