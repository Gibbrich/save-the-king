﻿using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.ui;
using Game.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public UiAnimatableElement backgroundPanel;
    public UiAnimatableElement startBattleImage;
    public UiAnimatableElement victoryBanner;
    public UiAnimatableElement levelFailedBanner;
    public UiAnimatableElement protectTheKingLabel;
    public UiAnimatableElement placeHumansLabel;
    public ProgressBarController levelProgress;
    public AvailableToSpawnController availableSoldiersToSpawn;

    public void SetState(UIManagerState state)
    {
        var uiState = GetUIState(state);
        SetUIState(uiState);
    }

    private UIState GetUIState(UIManagerState state)
    {
        switch (state)
        {
            case UIManagerState.Loose loose:
                return new UIState(
                    isLevelFailedLableVisible: true,
                    isBackgroundPanelVisible: true
                );
            case UIManagerState.PlaceHumans placeHumans:
                return new UIState(
                    isProgressBarVisible: true,
                    isPlaceHumansLabelVisible: true,
                    isAvailableToSpawnBannerVisible: true
                );
            case UIManagerState.StartBattle startBattle:
                return new UIState(
                    isProgressBarVisible: true,
                    isStartBattleImageVisible: true
                );
            case UIManagerState.Victory victory:
                return new UIState(
                    isVictoryBannerVisible: true,
                    isBackgroundPanelVisible: true
                );
            default:
                throw new ArgumentOutOfRangeException(nameof(state));
        }
    }

    private void SetUIState(UIState state)
    {
        if (state.isBackgroundPanelVisible)
        {
            backgroundPanel.Show(true);
        }
        else
        {
            backgroundPanel.Hide(true);
        }

        if (state.isProgressBarVisible)
        {
            levelProgress.Show(true);
        }
        else
        {
            levelProgress.Hide(true);
        }

        if (state.isVictoryBannerVisible)
        {
            victoryBanner.Show(true);
        }
        else
        {
            victoryBanner.Hide(true);
        }

        if (state.isLevelFailedLableVisible)
        {
            levelFailedBanner.Show(true);
        }
        else
        {
            levelFailedBanner.Hide(true);
        }

        if (state.isPlaceHumansLabelVisible)
        {
            placeHumansLabel.Show(true);
        }
        else
        {
            placeHumansLabel.Hide(true);
        }

        if (state.isProtectKingLableVisible)
        {
            protectTheKingLabel.Show(true);
        }
        else
        {
            protectTheKingLabel.Hide(true);
        }

        if (state.isStartBattleImageVisible)
        {
            startBattleImage.Show(true);
        }
        else
        {
            startBattleImage.Hide(true);
        }

        if (state.isAvailableToSpawnBannerVisible)
        {
            availableSoldiersToSpawn.Show(true);
        }
        else
        {
            availableSoldiersToSpawn.Hide(true);
        }
    }

    public void SetAvailableSoldiersToSpawnAmount(int amount)
    {
        availableSoldiersToSpawn.SetAvailableSoldiersToSpawnAmount(amount);
    }

    public void UpdateLevelInfo(int currentLevel)
    {
        levelProgress.UpdateLevelInfo(currentLevel);
    }

    public abstract class UIManagerState
    {
        public sealed class Victory : UIManagerState
        {
        }

        public sealed class Loose : UIManagerState
        {
        }

        public sealed class PlaceHumans : UIManagerState
        {
        }

        public sealed class StartBattle : UIManagerState
        {
        }
    }

    private readonly struct UIState
    {
        public readonly bool isStartBattleImageVisible;
        public readonly bool isVictoryBannerVisible;
        public readonly bool isLevelFailedLableVisible;
        public readonly bool isProtectKingLableVisible;
        public readonly bool isProgressBarVisible;
        public readonly bool isAvailableToSpawnBannerVisible;
        public readonly bool isBackgroundPanelVisible;
        public readonly bool isPlaceHumansLabelVisible;

        public UIState(
            bool isStartBattleImageVisible = false,
            bool isVictoryBannerVisible = false,
            bool isLevelFailedLableVisible = false,
            bool isProtectKingLableVisible = false,
            bool isProgressBarVisible = false,
            bool isAvailableToSpawnBannerVisible = false,
            bool isBackgroundPanelVisible = false,
            bool isPlaceHumansLabelVisible = false
        )
        {
            this.isStartBattleImageVisible = isStartBattleImageVisible;
            this.isVictoryBannerVisible = isVictoryBannerVisible;
            this.isLevelFailedLableVisible = isLevelFailedLableVisible;
            this.isProtectKingLableVisible = isProtectKingLableVisible;
            this.isProgressBarVisible = isProgressBarVisible;
            this.isAvailableToSpawnBannerVisible = isAvailableToSpawnBannerVisible;
            this.isBackgroundPanelVisible = isBackgroundPanelVisible;
            this.isPlaceHumansLabelVisible = isPlaceHumansLabelVisible;
        }
    }
}