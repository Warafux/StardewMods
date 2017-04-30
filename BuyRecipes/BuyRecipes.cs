﻿using Denifia.Stardew.BuyRecipes.Domain;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denifia.Stardew.BuyRecipes
{
    public class BuyRecipes : Mod
    {
        private bool _savedGameLoaded = false;
        private List<CookingRecipe> _cookingRecipes;
        private List<CookingRecipe> _thisWeeksRecipes;
        
        public static List<IRecipeAquisitionConditions> RecipeAquisitionConditions;

        public override void Entry(IModHelper helper)
        {
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveEvents.AfterReturnToTitle += SaveEvents_AfterReturnToTitle;

            RecipeAquisitionConditions = new List<IRecipeAquisitionConditions>()
            {
                new FriendBasedRecipeAquisition(),
                new SkillBasedRecipeAquisition(),
                new LevelBasedRecipeAquisition()
            };

            helper.ConsoleCommands.Add("buy", $"temp", HandleCommand);
        }

        private void HandleCommand(string command, string[] args)
        {
            if (!_savedGameLoaded)
            {
                Monitor.Log("Please load up a saved game first, then try again.", LogLevel.Warn);
                return;
            }

            switch (command)
            {
                case "buy":
                    BuyRecipe(args);
                    break;
                default:
                    throw new NotImplementedException($"Send Items received unknown command '{command}'.");
            }
        }

        private void BuyRecipe(string[] args)
        {
            
            if (args.Length == 1)
            {
                var recipeName = args[0];
                var recipe = _cookingRecipes.FirstOrDefault(x => x.Name == recipeName);
                if (recipe == null)
                {
                    Monitor.Log("Recipe not found", LogLevel.Info);
                    return;
                }

                if (recipe.IsKnown || Game1.player.cookingRecipes.ContainsKey(recipeName))
                {
                    recipe.IsKnown = true;
                    Monitor.Log("Recipe already known", LogLevel.Info);
                    return;
                }

                if (!_thisWeeksRecipes.Any(x => x.Name.Equals(recipeName)))
                {
                    Monitor.Log("Recipe is not availble to buy this week", LogLevel.Info);
                }

                Game1.player.cookingRecipes.Add(recipeName, 0);
                Game1.player.Money -= recipe.AquisitionConditions.Cost;
                Monitor.Log($"{recipeName} bought for ${recipe.AquisitionConditions.Cost}!", LogLevel.Alert);
            }
            else
            {
                LogArgumentsInvalid("buy");
            }
        }

        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            _savedGameLoaded = false;
            _cookingRecipes = null;
            _thisWeeksRecipes = null;
        }
        
        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            _savedGameLoaded = true;
            DiscoverRecipes();

            //foreach (var item in _cookingRecipes.Where(x => !x.IsKnown).OrderBy(x => x.AquisitionConditions.Cost))
            //{
            //    Monitor.Log($"{item.AquisitionConditions.Cost} - {item.Name}", LogLevel.Info);
            //}

            var gameDateTime = new GameDateTime(Game1.timeOfDay, Game1.dayOfMonth, Game1.currentSeason, Game1.year);
            var startDayOfWeek = (((gameDateTime.DayOfMonth / 7) + 1) * 7) - 6;
            var seed = int.Parse($"{startDayOfWeek}{gameDateTime.Season}{gameDateTime.Year}");
            var random = new Random(seed);

            _thisWeeksRecipes = new List<CookingRecipe>();
            var maxNumberOfRecipesPerWeek = 5;
            var unknownRecipes = _cookingRecipes.Where(x => !x.IsKnown).ToList();
            var unknownRecipesCount = unknownRecipes.Count;
            for (int i = 0; i < maxNumberOfRecipesPerWeek; i++)
            {
                var recipe = unknownRecipes[random.Next(unknownRecipesCount)];
                if (!_thisWeeksRecipes.Any(x => x.Name.Equals(recipe.Name)))
                {
                    _thisWeeksRecipes.Add(recipe);
                }
            }

            Monitor.Log($"This weeks recipes are:", LogLevel.Info);
            foreach (var item in _thisWeeksRecipes)
            {
                Monitor.Log($"{item.AquisitionConditions.Cost} - {item.Name}", LogLevel.Info);
            }
        }

        private void DiscoverRecipes()
        {
            var knownRecipes = Game1.player.cookingRecipes.Keys;
            _cookingRecipes = new List<CookingRecipe>();
            foreach (var recipe in CraftingRecipe.cookingRecipes)
            {
                var cookingRecipe = new CookingRecipe(recipe.Key, recipe.Value);
                if (Game1.player.cookingRecipes.ContainsKey(cookingRecipe.Name))
                {
                    cookingRecipe.IsKnown = true;
                }
                _cookingRecipes.Add(cookingRecipe);
            }

            var unknownRecipeCount = _cookingRecipes.Where(x => !x.IsKnown).Count();
        }

        private void LogUsageError(string error, string command)
        {
            Monitor.Log($"{error} Type 'help {command}' for usage.", LogLevel.Error);
        }

        private void LogArgumentsInvalid(string command)
        {
            LogUsageError("The arguments are invalid.", command);
        }
    }
}
