﻿using StardewValley;
using System.Collections.Generic;
using Denifia.Stardew.SendItems.Domain;
using Denifia.Stardew.SendItems.Menus;
using Denifia.Stardew.SendItems.Events;

namespace Denifia.Stardew.SendItems.Services
{
    public interface IPostboxService {
        void ShowComposeMailUI();
    }

    public class PostboxService : IPostboxService
    {
        private const string _leaveSelectionKeyAndValue = "(Leave)";
        private const string _messageFormat = "Hey there!^^  I thought you might like this... Take care! ^    -{0} %item object {1} {2} %%";

        private readonly IFarmerService _farmerService;
        private readonly IConfigurationService _configService;

        public PostboxService(
            IConfigurationService configService,
            IFarmerService farmerService)
        {
            _configService = configService;
            _farmerService = farmerService;

            ModEvents.MailComposed += MailComposed;
        }

        public void ShowComposeMailUI()
        {
            DisplayFriendSelector();
        }

        private void DisplayFriendSelector()
        {
            if (Game1.activeClickableMenu != null) return;
            List<Response> responseList = new List<Response>();
            foreach (var friend in new List<Friend>()) // TODO: Replace with real list
            {
                responseList.Add(new Response(friend.Id, friend.DisplayText));
            }
            responseList.Add(new Response(_leaveSelectionKeyAndValue, _leaveSelectionKeyAndValue));
            Game1.currentLocation.createQuestionDialogue("Select Friend:", responseList.ToArray(), new GameLocation.afterQuestionBehavior(FriendSelectorAnswered), (NPC)null);
            Game1.player.Halt();
        }

        private void FriendSelectorAnswered(StardewValley.Farmer farmer, string answer)
        {
            if (!answer.Equals(_leaveSelectionKeyAndValue))
            {
                var items = new List<Item>
                {
                    null
                };
                Game1.activeClickableMenu = new ComposeLetter(answer, items, 1, 1, null, HighlightOnlyGiftableItems);
                //Game1.activeClickableMenu = (IClickableMenu)new ComposeLetter(answer, items, 1, 1, new ComposeLetter.behaviorOnItemChange(onLetterChange)); // TODO: Should I use this instead?
            }
        }

        private bool HighlightOnlyGiftableItems(Item i)
        {
            return i.canBeGivenAsGift();
        }

        private void MailComposed(object sender, MailComposedEventArgs e)
        {
            var toFarmerId = e.ToFarmerId;
            var item = e.Item;

            if (item == null) return;

            var messageText = string.Format(_messageFormat, "farmerName", item.parentSheetIndex, item.getStack()); // TODO: add farmer name

            // TODO: Create mail in local DB and set it to Posted
        }
    }
}