﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VanillaTradingExpanded
{

    [HotSwappable]
    [StaticConstructorOnStartup]
    public class Window_PerformTransactionGains : Window
	{
		public TransactionProcess transactionProcess;
        public override Vector2 InitialSize => new Vector2(500, 430);

		public string title;

		public bool disableCloseButton = false;
        public Window_PerformTransactionGains(string title, TransactionProcess parent, bool disableCloseButton = false)
        {
			this.title = title;
			this.transactionProcess = parent;
			this.forcePause = true;
			this.transactionProcess.amountToTransfer = new Dictionary<Bank, int>();
			foreach (var bank in parent.allBanks)
            {
				this.transactionProcess.amountToTransfer[bank] = 0;
			}
			this.disableCloseButton = disableCloseButton;
		}
		public override void OnCancelKeyPressed()
        {

        }

        public string textEntryBuffer;
		private Vector2 scrollPosition;
		private float allMoneyToGain;
		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
			allMoneyToGain = this.transactionProcess.amountToTransfer.Sum(x => x.Value);
			var pos = new Vector2(inRect.x, inRect.y);
			var titleRect = new Rect(pos.x, pos.y, inRect.width, 48f);
			Widgets.Label(titleRect, title);
			pos.y += 48f;
			float listHeight = transactionProcess.allBanks.Count * 28f;
			Rect viewRect = new Rect(pos.x, pos.y, inRect.width, (inRect.height - pos.y) - 90);
			Rect scrollRect = new Rect(pos.x, pos.y, inRect.width - 16f, listHeight);
			Widgets.BeginScrollView(viewRect, ref scrollPosition, scrollRect);
			for (var i = 0; i < transactionProcess.allBanks.Count; i++)
            {
				var bank = transactionProcess.allBanks[i];
				Text.Anchor = TextAnchor.MiddleLeft;
				var entryRect = new Rect(pos.x, pos.y, inRect.width, 24);
				if (i % 2 == 1)
				{
					Widgets.DrawLightHighlight(entryRect);
				}
				var bankIcon = new Rect(pos.x, pos.y, 24, 24);
				GUI.color = bank.parentFaction.Color;
				GUI.DrawTexture(bankIcon, bank.parentFaction.def.FactionIcon);
				GUI.color = Color.white;

				var bankLabelRect = new Rect(bankIcon.xMax + 10, pos.y, 200, 24);
				Widgets.Label(bankLabelRect, bank.Name);
				var depositAmountRect = new Rect(bankLabelRect.xMax, pos.y, 65, 24);
				Widgets.Label(depositAmountRect, bank.DepositAmount.ToStringMoney());
				var withdrawFullyRect = new Rect(depositAmountRect.xMax, pos.y, 24, 24);
				var amountMoneyExceptThisBank = this.transactionProcess.amountToTransfer.Where(x => x.Key != bank).Sum(x => x.Value);
				if (Widgets.ButtonText(withdrawFullyRect, "<<") && transactionProcess.transactionGain > this.transactionProcess.amountToTransfer.Where(x => x.Key != bank).Sum(x => x.Value))
				{
					this.transactionProcess.amountToTransfer[bank] = transactionProcess.transactionGain - amountMoneyExceptThisBank;
				}
				var withdrawRect = new Rect(withdrawFullyRect.xMax, pos.y, 24, 24);
				if (Widgets.ButtonText(withdrawRect, "<") && this.transactionProcess.transactionGain - allMoneyToGain > 0)
				{
					this.transactionProcess.amountToTransfer[bank] += 1 * GenUI.CurrentAdjustmentMultiplier();
				}
				var textEntry = new Rect(withdrawRect.xMax + 5, pos.y, 60, 24);
				textEntryBuffer = this.transactionProcess.amountToTransfer[bank].ToString();
				var value = this.transactionProcess.amountToTransfer[bank];
				Widgets.TextFieldNumeric<int>(textEntry, ref value, ref textEntryBuffer, 0, (transactionProcess.transactionGain - amountMoneyExceptThisBank));
				this.transactionProcess.amountToTransfer[bank] = value;

				var depositRect = new Rect(textEntry.xMax + 5, pos.y, 24, 24);
				if (Widgets.ButtonText(depositRect, ">") && this.transactionProcess.amountToTransfer[bank] > 0)
				{
					this.transactionProcess.amountToTransfer[bank] -= 1 * GenUI.CurrentAdjustmentMultiplier();
				}
				var depositFullyRect = new Rect(depositRect.xMax, pos.y, 24, 24);
				if (Widgets.ButtonText(depositFullyRect, ">>"))
				{
					this.transactionProcess.amountToTransfer[bank] = 0;
				}
				GUI.color = Color.white;
				pos.y += 28f;
			}
			Widgets.EndScrollView();

			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;

			pos.y = inRect.height - 60;
			var transactionGainToExtract = new Rect(pos.x, pos.y, 250, 24);
			Widgets.Label(transactionGainToExtract, "VTE.TransactionGainToTransfer".Translate(transactionProcess.transactionGain));
			
			var moneyToBePaid = new Rect(transactionGainToExtract.xMax + 40, pos.y, 250, 24);
			Widgets.Label(moneyToBePaid, "VTE.MoneyToTransfer".Translate(allMoneyToGain));

			pos.y += 30;
			bool canPay = allMoneyToGain == transactionProcess.transactionGain;
			GUI.color = canPay ? Color.white : Color.grey;
			var confirmButtonRect = new Rect(pos.x + 15, pos.y, 170, 32f);
			if (Widgets.ButtonText(confirmButtonRect, "Confirm".Translate(), active: canPay))
			{
				if (this.transactionProcess.transactionCost > 0)
                {
					Find.WindowStack.Add(new Window_PerformTransactionCosts("VTE.BankDepositsToSpend".Translate(), this.transactionProcess));
				}
				else
                {
					this.transactionProcess.PerformTransaction();
                }
				this.Close();
			}
			GUI.color = Color.white;
			var closeButtonRect = new Rect(confirmButtonRect.xMax + 85, confirmButtonRect.y, confirmButtonRect.width, confirmButtonRect.height);
			if (disableCloseButton is false && Widgets.ButtonText(closeButtonRect, "Close".Translate()))
			{
				transactionProcess.PostCancel();
				this.Close();
			}
		}
		public override void PostClose()
		{
			base.PostClose();
			transactionProcess.PostClose();
		}
	}
}
