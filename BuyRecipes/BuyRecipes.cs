﻿using StardewModdingAPI;
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
        private List<CookingRecipe> CookingRecipes;
        private static List<GameItem> _gameObjects;
        public static List<GameItem> GameObjects
        {
            get
            {
                if (_gameObjects == null)
                {
                    DeserializeGameObjects();
                }
                return _gameObjects;
            }
        }
        public static List<IRecipeAquisitionConditions> RecipeAquisitionConditions;

        public override void Entry(IModHelper helper)
        {
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveEvents.AfterReturnToTitle += SaveEvents_AfterReturnToTitle;

            RecipeAquisitionConditions = new List<IRecipeAquisitionConditions>()
            {
                new FriendBasedRecipeAquisition()
            };
        }

        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            ResetRecipes();
        }

        private void ResetRecipes()
        {
            CookingRecipes = null;
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            DiscoverRecipes();

            foreach (var item in CookingRecipes.OrderBy(x => x.AquisitionConditions.Cost))
            {
                this.Monitor.Log($"{item.AquisitionConditions.Cost} - {item.Name}", LogLevel.Info);
            }

            //Game1.player.cookingRecipes.Add("name", 0);
        }

        private void DiscoverRecipes()
        {
            var knownRecipes = Game1.player.cookingRecipes.Keys;
            CookingRecipes = new List<CookingRecipe>();
            foreach (var recipe in CraftingRecipe.cookingRecipes)
            {
                var cookingRecipe = new CookingRecipe(recipe.Key, recipe.Value);
                if (Game1.player.cookingRecipes.ContainsKey(cookingRecipe.Name))
                {
                    cookingRecipe.IsKnown = true;
                }
                CookingRecipes.Add(cookingRecipe);
            }

            var unknownRecipeCount = CookingRecipes.Where(x => !x.IsKnown).Count();
        }

        private static void DeserializeGameObjects()
        {
            _gameObjects = new List<GameItem>();
            foreach (var item in Game1.objectInformation)
            {
                _gameObjects.Add(new GameItem
                {
                    Id = item.Key,
                    Name = item.Value.Split('/')[4]
                });
            }
        }
    }

    public class CookingRecipe
    {
        public string Name { get; set; }
        public List<GameItemWithQuantity> Ingredients { get; set; }
        public GameItemWithQuantity ResultingItem { get; set; }
        public IRecipeAquisitionConditions AquisitionConditions { get; set; }
        public bool IsKnown { get; set; }

        public CookingRecipe(string name, string data)
        {
            Name = name;

            var dataParts = data.Split('/');

            var ingredientsData = dataParts[0];
            Ingredients = DeserializeIngredients(ingredientsData);

            var unknownData = dataParts[1];

            var resultingItemData = dataParts[2];
            ResultingItem = DeserializeResultingItem(resultingItemData);

            var aquisitionData = dataParts[3];
            var aquisitionConditions = BuyRecipes.RecipeAquisitionConditions.FirstOrDefault(x => x.AcceptsConditions(aquisitionData));
            if (aquisitionConditions == null)
            {
                AquisitionConditions = new DefaultRecipeAquisition(aquisitionData);
            }
            else
            {
                AquisitionConditions = (IRecipeAquisitionConditions)Activator.CreateInstance(aquisitionConditions.GetType(), new object[] { aquisitionData });
            }
        }

        private List<GameItemWithQuantity> DeserializeIngredients(string data)
        {
            var ingredients = new List<GameItemWithQuantity>();
            var dataParts = data.Split(' ');
            for (int i = 0; i < dataParts.Count(); i++)
            {
                try
                {
                    var ingredientData = DeserializeItemWithQuantity(dataParts[i], dataParts[i + 1]);
                    ingredients.Add(ingredientData);

                    i++; // Skip in pairs
                }
                catch (Exception ex)
                {
                }
            }
            return ingredients;
        }

        private GameItemWithQuantity DeserializeResultingItem(string data)
        {
            var dataParts = data.Split(' ');
            if (dataParts.Count() == 1)
            {
                // Default amount of an item is 1
                return DeserializeItemWithQuantity(dataParts[0], "1");
            }
            return DeserializeItemWithQuantity(dataParts[0], dataParts[1]);
        }

        private GameItemWithQuantity DeserializeItemWithQuantity(string itemId, string quantity)
        {
            var itemWithQuantity = new GameItemWithQuantity
            {
                Id = int.Parse(itemId),
                Quantity = int.Parse(quantity),
            };

            var gameItem = BuyRecipes.GameObjects.FirstOrDefault(x => x.Id == itemWithQuantity.Id);
            if (gameItem != null)
            {
                itemWithQuantity.Name = gameItem.Name;
            }

            return itemWithQuantity;
        }
    }

    public class GameItem
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }

    public class GameItemWithQuantity : GameItem
    {
        public int Quantity { get; set; }
    }

    public interface IRecipeAquisitionConditions
    {
        bool AcceptsConditions(string condition);
        int Cost { get; }
    }

    public abstract class BaseRecipeAquisition
    {
        public BaseRecipeAquisition()
        {
        }

        public BaseRecipeAquisition(string data)
        {
        }
    }

    public class FriendBasedRecipeAquisition : BaseRecipeAquisition, IRecipeAquisitionConditions
    {
        private int _friendLevel;
        private string _friend;
        public int Cost => _friendLevel * 600;

        public FriendBasedRecipeAquisition() { }

        public FriendBasedRecipeAquisition(string data) : base(data)
        {
            var dataParts = data.Split(' ');
            _friend = dataParts[1];
            _friendLevel = int.Parse(dataParts[2]);
        }

        public bool AcceptsConditions(string condition)
        {
            return condition.StartsWith("f ");
        }
    }

    public class DefaultRecipeAquisition : BaseRecipeAquisition, IRecipeAquisitionConditions
    {
        public int Cost => 500;

        public DefaultRecipeAquisition() { }

        public DefaultRecipeAquisition(string data) : base(data) { }

        public bool AcceptsConditions(string condition)
        {
            return true;
        }
    }
}
