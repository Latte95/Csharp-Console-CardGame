using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace weekend_homeWork_1_poker
{
  public enum HandRank
  {
    노카드,  // 족보 비교 때 첫 플레이어가 하이카드일 때를 대비해서 구현했는데 없어도 될 듯 싶음
    하이카드,
    원페어,
    투페어,
    트리플,
    스트레이트,
    플러쉬,
    풀하우스,
    포카드,
    스트레이트플러쉬
  }

  // 덱을 만들어 셔플하고, 카드를 분배하고, 배팅금을 관리하며, 플레이어의 결과를 비교하는 클래스
  class Dealer
  {
    private Deck deck;
    public int CurrentBettingMoney { get; set; }
    public int TotalBettingMoney { get; set; }

    public Dealer()
    {
      deck = new Deck();
      CurrentBettingMoney = 0;
      TotalBettingMoney = 0;
    }

    public void ShuffleDeck()
    {
      deck.Shuffle();
    }

    // 플레이어에게 카드를 나눠주는 메서드
    public void DealCard(Player player)
    {
      // 덱에서 카드 한장을 뽑아서
      Card card = deck.DrawCard();
      // 플레이어에게 분배함
      player.Hand.AddCard(card);
    }

    // 
    public int GetDeckSize()
    {
      return deck.Size;
    }

    #region 플레이어 패 판단
    public HandRank EvaluateHand(Player player)
    {
      int[] rankCount = new int[Enum.GetValues(typeof(Rank)).Length + 2]; // ace가 13이기 때문에 +2, 0과 1인덱스엔 저장x
      int[] pairCount = new int[Enum.GetValues(typeof(Suit)).Length + 1];
      int[] suitCount = new int[Enum.GetValues(typeof(Suit)).Length];
      bool isFlush = false;
      bool isStraight = false;
      int flushSuit = 0;
      int straightRank = 0;

      // 각 문양과 숫자가 몇개인지 저장
      foreach (Card card in player.Hand.Cards)
      {
        rankCount[(int)card.Rank]++;
        suitCount[(int)card.Suit]++;
      }
      // 같은 숫자 수 저장
      foreach (int i in rankCount)
      {
        // 2장 있는 카드는 [2]에 저장하는 식으로 페어수 구분
        pairCount[i]++;
      }

      // 플러쉬 판단
      for (int i = 0; i < Enum.GetValues(typeof(Suit)).Length; i++)
      {
        // 같은 모양이 5장 있으면
        if (suitCount[i].Equals(5))
        {
          // 플러쉬고
          isFlush = true;
          // 플러쉬인 문양 저장
          flushSuit = i;
        }
      }
      // 스트레이트 판단
      for (int i = (int)Rank.Two; i <= (int)Rank.Ace; i++)
      {
        int[] rankNum = new int[5];
        for (int j = 0; j < rankNum.Length; j++)
        {
          if (i + j >= (int)Rank.Ace)
          {
            rankNum[j] = i + j - (int)Rank.Ace + 1;
          }
          else
          {
            rankNum[j] = i + j;
          }
        }
        // 5장이 연달아 있으면 스트레이트
        // 13인 ACE와 2인 TWO를 연결하기 위해 %14 이용. %13을 이용할 경우 ace가 0으로 되어 
        if (rankCount[rankNum[0]] > 0 &&
            rankCount[rankNum[1]] > 0 &&
            rankCount[rankNum[2]] > 0 &&
            rankCount[rankNum[3]] > 0 &&
            rankCount[rankNum[4]] > 0)
        {
          isStraight = true;
          // 가장 높은 숫자. A로 시작하거나 중간에 A가 끼면 A가 가장 높음
          if (i.Equals(0) || i + 4 > 13)
          {
            straightRank = 13;
          }
          // A가 포함 안된다면 i+4가 가장 높은 숫자
          else
          {
            straightRank = i + 4;
          }
        }
      }

      // 플러쉬+스트레이트면 스티플
      if (isFlush && isStraight)
      {
        player.highSuit = flushSuit;
        player.highRank = straightRank;
        return HandRank.스트레이트플러쉬;
      }
      // 4장인 카드가 있으면 포카드
      else if (pairCount[4].Equals(1))
      {
        for (int i = 1; i < rankCount.Length; i++)
        {
          // 4장 있는 rank를 찾아서
          if (rankCount[i].Equals(4))
          {
            //rank를 저장하고, suit는 스페이드로 저장함
            player.highRank = i;
            player.highSuit = (int)Suit.Spades;
          }
        }
        return HandRank.포카드;
      }
      // 풀하우스 = 트리플+원페어
      else if (pairCount[3].Equals(1) && pairCount[2].Equals(1))
      {
        for (int i = 1; i < rankCount.Length; i++)
        {
          if (rankCount[i].Equals(3))
          {
            player.highRank = i;
            player.highSuit = -1;
            foreach (Card card in player.Hand.Cards)
            {
              if ((int)card.Rank == i && (int)card.Suit > player.highSuit)
              {
                player.highSuit = (int)card.Suit;
              }
            }
          }
        }
        return HandRank.풀하우스;
      }
      else if (isFlush)
      {
        player.highSuit = flushSuit;
        player.highRank = -1;
        // Rank는 플러쉬를 완성한 모양중에 가장 높은 rank
        foreach (Card card in player.Hand.Cards)
        {
          if ((int)card.Suit == flushSuit && (int)card.Rank > player.highRank)
          {
            player.highRank = (int)card.Rank;
          }
        }
        return HandRank.플러쉬;
      }
      else if (isStraight)
      {
        player.highRank = straightRank;
        player.highSuit = -1;
        // 모양은 가장 높은 rank의 모양
        foreach (Card card in player.Hand.Cards)
        {
          if ((int)card.Rank == player.highRank && (int)card.Suit > player.highSuit)
          {
            player.highSuit = (int)card.Suit;
          }
        }
        return HandRank.스트레이트;
      }
      // 3장인 카드가 있으면 트리플
      else if (pairCount[3].Equals(1))
      {
        for (int i = 1; i < rankCount.Length; i++)
        {
          // 3장있는 rank를 찾아서
          if (rankCount[i].Equals(3))
          {
            // rank 설정하고
            player.highRank = i;
            player.highSuit = -1;
            // suit은 해당 rank의 모양중 가장 높은 것으로 설정함
            foreach (Card card in player.Hand.Cards)
            {
              if ((int)card.Rank == i && (int)card.Suit > player.highSuit)
              {
                player.highSuit = (int)card.Suit;
              }
            }
          }
        }
        return HandRank.트리플;
      }
      //  2장인 카드 2쌍이면 투페어
      else if (pairCount[2].Equals(2))
      {
        player.highRank = -1;
        for (int i = 1; i < rankCount.Length; i++)
        {
          if (rankCount[i].Equals(2))
          {
            if (i > player.highRank)
            {
              player.highRank = i;
              player.highSuit = -1;
              foreach (Card card in player.Hand.Cards)
              {
                if ((int)card.Rank == i && (int)card.Suit > player.highSuit)
                {
                  player.highSuit = (int)card.Suit;
                }
              }
            }
          }
        }
        return HandRank.투페어;
      }
      // 2장인 카드 1쌍이면 원페어
      else if (pairCount[2].Equals(1))
      {
        for (int i = 1; i < rankCount.Length; i++)
        {
          if (rankCount[i].Equals(2))
          {
            player.highRank = i;
            player.highSuit = -1;
            foreach (Card card in player.Hand.Cards)
            {
              if ((int)card.Rank == i && (int)card.Suit > player.highSuit)
              {
                player.highSuit = (int)card.Suit;
              }
            }
          }
        }
        return HandRank.원페어;
      }
      // 아무것도 없으면 하이카드
      else
      {
        for (int i = 1; i < rankCount.Length; i++)
        {
          if (rankCount[i].Equals(1))
          {
            player.highRank = i;
            player.highSuit = -1;
            foreach (Card card in player.Hand.Cards)
            {
              if ((int)card.Rank == i && (int)card.Suit > player.highSuit)
              {
                player.highSuit = (int)card.Suit;
              }
            }
          }
        }
        return HandRank.하이카드;
      }
    }

    public Player WinnerDetermine(List<Player> players)
    {
      Player winner = null;
      HandRank highestRank = HandRank.노카드;

      // 플레이어중 가장 높은 패를 가지고 있는 사람을 저장
      foreach (Player player in players)
      {
        // 모두 다이여서 이미 승자가 판별됐을 경우 종료
        if (player.isWin)
        {
          return player;
        }
        // 죽었거나 파산한 플레이어는 스킵
        if (player.isDie || player.isBankrupt)
        {
          continue;
        }

        // 현재 플레이어 족보가
        HandRank currentHandRank = EvaluateHand(player);
        // 이전까지 가장 높은 족보보다 좋으면
        if (currentHandRank > highestRank)
        {
          // 현재 플레이어 족보와 정보 저장
          highestRank = currentHandRank;
          winner = player;
        }
        // 다른 플레이어와 족보가 똑같으면
        else if (currentHandRank == highestRank)
        {
          // 랭크가 더 높은지 비교하고
          if (player.highRank > winner.highRank)
          {
            winner = player;
          }
          // 랭크마저 똑같으면 모양을 비교함
          else if (player.highRank.Equals(winner.highRank))
          {
            if (player.highSuit > winner.highSuit)
            {
              winner = player;
            }
          }
        }
      }
      winner.isWin = true;
      return winner;
    }

    public void OpenAllHands(List<Player> players)
    {
      Console.WriteLine();
      Console.WriteLine("모든 플레이어의 패를 공개합니다.");
      Console.WriteLine();
    }
    #endregion
  }
}
