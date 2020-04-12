﻿using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace SoundTheAlarm {
    public class STAAction {

        private Dictionary<string, float> managedSettlements;
        private Settlement settlementToTrack;

        public void Initialize() {
            managedSettlements = new Dictionary<string, float>();
        }

        // Action method fired once the VilageBeingRaided event fires
        public void DisplayVillageRaid(Village v) {
            RemoveExpiredFromManagedSettlements(v.Settlement);
            if (Hero.MainHero != null) {
                if (Hero.MainHero.IsAlive) {
                    if (ShouldAlertForSettlement(v.Settlement)) {
                        if (!managedSettlements.ContainsKey(v.Settlement.Name.ToString())) {
                            managedSettlements.Add(v.Settlement.Name.ToString(), Campaign.CurrentTime);
                            string display =
                                "The village of " + 
                                v.Settlement.Name.ToString() +
                                " is under attack by " +
                                v.Settlement.LastAttackerParty.Name.ToString() +
                                " of the " +
                                v.Settlement.LastAttackerParty.LeaderHero.MapFaction.Name.ToString() +
                                "!"
                            ;

                            settlementToTrack = v.Settlement;
                            InformationManager.ShowInquiry(new InquiryData("Village Being Raided", display, true, true, "Track", "Close", new Action(Track), null, ""), STALibrary.Instance.STAConfiguration.PauseGameOnPopup);
                            if (STALibrary.Instance.STAConfiguration.EnableDebugMessages)
                                InformationManager.DisplayMessage(new InformationMessage("STALibrary: " + display, new Color(1.0f, 0.0f, 0.0f)));
                        }
                    }
                }
            }
        }

        // Action method fired once the VillageBecomeNormal event fires
        public void FinalizeVillageRaid(Village v) {
            RemoveExpiredFromManagedSettlements(v.Settlement);
        }

        // Action method fired once the OnSiegeEventStartedEvent event fires
        public void DisplaySiege(SiegeEvent e) {
            if (Hero.MainHero != null) {
                if (Hero.MainHero.IsAlive) {
                    if (ShouldAlertForSettlement(e.BesiegedSettlement)) {
                        if(e.BesiegedSettlement.IsCastle && STALibrary.Instance.STAConfiguration.EnableCastlePopup) {
                            if (!managedSettlements.ContainsKey(e.BesiegedSettlement.Name.ToString())) {
                                managedSettlements.Add(e.BesiegedSettlement.Name.ToString(), 0.0f);
                                string display =
                                    "The castle of " + 
                                    e.BesiegedSettlement.Name.ToString() +
                                    " is under attack by " +
                                    e.BesiegedSettlement.LastAttackerParty.Name.ToString() +
                                    " of the " +
                                    e.BesiegedSettlement.LastAttackerParty.LeaderHero.MapFaction.Name.ToString() +
                                    "!"
                                ;
                                settlementToTrack = e.BesiegedSettlement;
                                InformationManager.ShowInquiry(new InquiryData("Castle Under Siege", display, true, true, "Track", "Close", new Action(Track), null, ""), STALibrary.Instance.STAConfiguration.PauseGameOnPopup);
                                if (STALibrary.Instance.STAConfiguration.EnableDebugMessages)
                                    InformationManager.DisplayMessage(new InformationMessage("STALibrary: " + display, new Color(1.0f, 0.0f, 0.0f)));
                            }
                        } else if(e.BesiegedSettlement.IsTown && STALibrary.Instance.STAConfiguration.EnableTownPopup) {
                            managedSettlements.Add(e.BesiegedSettlement.Name.ToString(), 0.0f);
                            string display =
                                "The town of " + 
                                e.BesiegedSettlement.Name.ToString() +
                                " is under attack by " +
                                e.BesiegedSettlement.LastAttackerParty.Name.ToString() +
                                " of the " +
                                e.BesiegedSettlement.LastAttackerParty.LeaderHero.MapFaction.Name.ToString() +
                                "!"
                            ;
                            settlementToTrack = e.BesiegedSettlement;
                            InformationManager.ShowInquiry(new InquiryData("Town Under Siege", display, true, true, "Track", "Close", new Action(Track), null, ""), STALibrary.Instance.STAConfiguration.PauseGameOnPopup);
                            if (STALibrary.Instance.STAConfiguration.EnableDebugMessages)
                                InformationManager.DisplayMessage(new InformationMessage("STALibrary: " + display, new Color(1.0f, 0.0f, 0.0f)));
                        }
                    }
                }
            }
        }

        // Action method fired once the OnSiegeEventEndedEvent event fires
        public void FinalizeSiege(SiegeEvent e) {
            if (managedSettlements.ContainsKey(e.BesiegedSettlement.Name.ToString())) {
                managedSettlements.Remove(e.BesiegedSettlement.Name.ToString());
                if (STALibrary.Instance.STAConfiguration.EnableDebugMessages)
                    InformationManager.DisplayMessage(new InformationMessage("STALibrary: Removed " + e.BesiegedSettlement.Name.ToString() + " from managed settlements list", new Color(1.0f, 0.0f, 0.0f)));
            }
        }

        // Action method fired once two empires declare war
        public void OnDeclareWar(IFaction faction1, IFaction faction2) {
            string display =
                faction1.Leader.Name.ToString() +
                " of the " +
                faction1.Name.ToString() +
                " has signed a declaration of war against the " +
                faction2.Name.ToString();
            ;
            InformationManager.ShowInquiry(new InquiryData("Declaration of War", display, true, false, "Ok", "Close", null, null, ""), STALibrary.Instance.STAConfiguration.PauseGameOnPopup);
            if (STALibrary.Instance.STAConfiguration.EnableDebugMessages)
                InformationManager.DisplayMessage(new InformationMessage("STALibrary: " + display, new Color(1.0f, 0.0f, 0.0f)));
        }

        // Action method fired once two empires declare peace
        public void OnDeclarePeace(IFaction faction1, IFaction faction2) {
            string display =
                faction1.Leader.Name.ToString() +
                " of the " +
                faction1.Name.ToString() +
                " has signed a declaration of peace with the " +
                faction2.Name.ToString();
            ;
            InformationManager.ShowInquiry(new InquiryData("Declaration of Peace", display, true, false, "Ok", "Close", null, null, ""), STALibrary.Instance.STAConfiguration.PauseGameOnPopup);
            if (STALibrary.Instance.STAConfiguration.EnableDebugMessages)
                InformationManager.DisplayMessage(new InformationMessage("STALibrary: " + display, new Color(1.0f, 0.0f, 0.0f)));
        }

        // Action method fired once the user clicks 'Track' on the popup
        public void Track() {
            Campaign.Current.VisualTrackerManager.RegisterObject(settlementToTrack);
            if (STALibrary.Instance.STAConfiguration.EnableDebugMessages)
                InformationManager.DisplayMessage(new InformationMessage("STALibrary: Tracking " + settlementToTrack.Name.ToString(), new Color(1.0f, 0.0f, 0.0f)));
        }

        // Check if the alert should fire (thanks to iPherian for submitting pull request that fixed alert not showing if you are not the king)
        private bool ShouldAlertForSettlement(Settlement settlement) {
            return settlement.MapFaction.Leader == Hero.MainHero || settlement.OwnerClan.Leader == Hero.MainHero;
        }

        // We ignore certain settlements when alerting for a period of time after user first alerted. This removes a settlement which has expired in that way from the list.
        private void RemoveExpiredFromManagedSettlements(Settlement settlement) {
            if (managedSettlements.ContainsKey(settlement.Name.ToString())) {
                float time;
                if (!managedSettlements.TryGetValue(settlement.Name.ToString(), out time)) {
                    return;
                }
                if (Campaign.CurrentTime > time + STALibrary.Instance.STAConfiguration.TimeToRemoveVillageFromList) {
                    managedSettlements.Remove(settlement.Name.ToString());
                    if (STALibrary.Instance.STAConfiguration.EnableDebugMessages) 
                        InformationManager.DisplayMessage(new InformationMessage("STALibrary: Removed " + settlement.Name.ToString() + " from managed settlements list", new Color(1.0f, 0.0f, 0.0f)));
                }
        
                if (STALibrary.Instance.STAConfiguration.EnableDebugMessages)
                    InformationManager.DisplayMessage(new InformationMessage("STALibrary: " + settlement.Name.ToString() + " count is at " + ((time + STALibrary.Instance.STAConfiguration.TimeToRemoveVillageFromList) - Campaign.CurrentTime), new Color(1.0f, 0.0f, 0.0f)));
            }
        }
    }
}
