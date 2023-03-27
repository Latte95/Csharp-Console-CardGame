using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace weekend_homeWork_1_poker
{
  class WalNamPong
  {
    const string WIN = "승리";
    const string LOSS = "패배";

    private Player player;
    private Player opponent;
    private Dealer dealer;
    private bool openAllHands;

    public void Play()
    {
      string card;

      Start();

      while (true)
      {
        Console.WriteLine("카드를 분배합니다.");
        dealer.DealCard(opponent);
        dealer.DealCard(opponent);
        dealer.DealCard(player);

        Console.ReadKey(true);
        Console.Clear();
        if (dealer.GetDeckSize() < 3)
        {
          Console.WriteLine("카드를 모두 소진하였습니다. 게임을 종료합니다.");
          return;
        }
        openAllHands = false;
        Console.WriteLine($"소지금 : {player.Money}");
        Console.WriteLine();

        card = Card.GetCardDisplayString(player.Hand.Cards[0]);
        Console.Write("딜러의 카드 : | hidden || hidden |　　　　　　");
        Console.Write($"|{card}|");
        Console.WriteLine();
        int battingMoney;
        // 배팅
        while (true)
        {
          Console.Write($"기본 배팅금액은 {Poker.DEFAULT_BET} 입니다.");
          Console.Write("배팅금을 입력하세요 : ");
          int.TryParse(Console.ReadLine(), out battingMoney);
          // 배팅금 범위 벗어나면 재입력
          if (battingMoney < Poker.DEFAULT_BET || battingMoney > player.Money)
          {
            continue;
          }
          // 잘 입력 했으면 입력 종료
          else
          {
            dealer.CurrentBettingMoney = battingMoney;
            break;
          }
        }

        Console.Clear();

        Console.WriteLine($"소지금 : {player.Money}");
        Console.WriteLine();
        card = Card.GetCardDisplayString(opponent.Hand.Cards[0]);
        Console.Write($"딜러의 카드 : |{card}|");
        card = Card.GetCardDisplayString(opponent.Hand.Cards[1]);
        Console.Write($"|{card}|　　　　　　");
        card = Card.GetCardDisplayString(player.Hand.Cards[0]);
        Console.Write($"|{card}|");

        string winORloss = WinOrLoss(opponent.Hand.Cards[0], opponent.Hand.Cards[1], player.Hand.Cards[0]);

        Result(winORloss);
      }
    }

    public void Start()
    {
      dealer = new Dealer();
      Console.Clear();
      Console.WriteLine("월남뽕 게임을 시작합니다.");
      CreatePlayer(2);
      Console.WriteLine("딜러가 카드를 셔플합니다.");
      dealer.ShuffleDeck();
    }
    public void CreatePlayer(int playerNum)
    {
      opponent = new Computer($"딜러");
      player = new User("플레이어");
    }
    public string WinOrLoss(Card dealerCard1, Card dealerCard2, Card playerCard)
    {
      int minNum = (int)dealerCard1.Rank;
      int maxNum = (int)dealerCard2.Rank;
      int playerNum = (int)playerCard.Rank;
      int temp;

      if (minNum.Equals(14))
      {
        minNum = 1;
      }
      if (maxNum.Equals(14))
      {
        maxNum = 1;
      }

      // 딜러 카드의 최소값과 최대값 정렬
      if (minNum > maxNum)
      {
        temp = minNum;
        minNum = maxNum;
        maxNum = temp;
      }

      if (playerNum >= minNum && playerNum <= maxNum)
        return WIN;
      else
        return LOSS;
    }

    public void Result(string winORloss)
    {
      Console.WriteLine();
      switch (winORloss)
      {
        case WIN:
          player.SetMoney(player.Money + dealer.CurrentBettingMoney);
          Console.WriteLine($"축하합니다! 당신은 이겼습니다.     ");
          break;
        case LOSS:
          player.SetMoney(player.Money - dealer.CurrentBettingMoney);
          Console.WriteLine($"당신은 졌습니다.                    ");
          break;
      }
      dealer.CurrentBettingMoney = 0;
      player.Hand.ClearHand();
      opponent.Hand.ClearHand();
    }

  }
}
