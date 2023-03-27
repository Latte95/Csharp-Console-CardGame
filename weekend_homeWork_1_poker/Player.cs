using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace weekend_homeWork_1_poker
{
  abstract class Player
  {
    #region 변수
    const int SEED_MONEY = 100000;                // 기본 소지금
    public const int MIN_BATTING_UNIT = 1000;     // 배팅 단위

    public Hand Hand { get; private set; }        // 들고있는 패
    public string Name { get; set; }              // 이름
    public int Money { get; private set; }        // 소지금
    public int PreviousBettingMoney { get; set; } // 가장 최근에 배팅한 금액. 배팅이 여러번 돌경우 얼마를 추가 배팅할지 결정하기 위한 변수.
    public int highRank { get; set; }             // 가장 높은 족보의 숫자  
    public int highSuit { get; set; }             // 가장 높은 족보의 문양
    public bool isDie { get; set; }               // 다이 외쳤는지?
    public bool isBankrupt { get; set; }          // 파산했는지?
    public bool isWin { get; set; }               // 이겼는지?
    public void SetMoney(int value)
    {
      if (value < 0)
      {
        Money = 0;
      }
      else
      {
        Money = value;
      }
    }
    #endregion

    public Player(string name)
    {
      Hand = new Hand();
      this.Name = name;
      Money = SEED_MONEY;
      PreviousBettingMoney = 0;
      isDie = false;
      isBankrupt = false;
      isWin = false;
    }

    // 배팅 옵션 선택. 자신과 컴퓨터의 선택 방식이 다르기 때문에 추상 메서드로 선언. (나 = 선택, 컴퓨터 = 랜덤)
    public abstract int BattingSelect(Dealer dealer, int selectBattingOption);
    // 배팅금 입력. 마찬가지로 배팅 금액도 다른 방식으로 설정하기 때문에 추상 메서드로 선언
    public abstract void BattingMoney(Dealer dealer, ref int callNum);
    // 하프를 외쳤을 경우
    public void Half(Dealer dealer, ref int callNum)
    {
      // 배팅금을 2배로 올림
      int battingMoney = dealer.CurrentBettingMoney * 2;
      // 2배로 올릴 돈이 있으면 그대로 배팅하고
      if (Money >= battingMoney)
      {
        SetBattingMoney(dealer, ref callNum, battingMoney);
      }
      // 없으면 전재산을 배팅함
      else
      {
        SetBattingMoney(dealer, ref callNum, Money);
      }
    }
    // 콜을 외쳤을 경우
    public void Call(Dealer dealer, ref int callNum)
    {
      // 돈이 충분히 있으면 배팅금 만큼 소지금을 차감하고
      if (Money > dealer.CurrentBettingMoney)
      {
        dealer.TotalBettingMoney += dealer.CurrentBettingMoney - PreviousBettingMoney;
        SetMoney(Money - (dealer.CurrentBettingMoney - PreviousBettingMoney));
        PreviousBettingMoney = dealer.CurrentBettingMoney;
      }
      // 없으면 보유금만큼만 가져감
      else
      {
        dealer.TotalBettingMoney += Money;
        SetMoney(0);
      }
      // 콜했으니 콜한 인원 증가
      callNum++;
    }
    // 하프, 배팅 등 금액을 올렸을 때 배팅금 설정
    public virtual void SetBattingMoney(Dealer dealer, ref int callNum, int battingMoney)
    {
      // 이전 배팅금은 벌써 냈기 때문에 새로 외친 배팅금에서 이전 배팅금을 빼줌
      dealer.TotalBettingMoney += battingMoney - PreviousBettingMoney;
      // 새로 외친 배팅금 갱신
      dealer.CurrentBettingMoney = battingMoney;
      // 소지금 갱신. 마찬가지로 이전 배팅턴에 냈던 금액은 제외하여 갱신함
      Money -= battingMoney - PreviousBettingMoney;
      // 배팅을 새로 했기 때문에 이전 배팅금을 갱신
      PreviousBettingMoney = battingMoney;
      // 배팅을 선언했으므로 call을 외친 인원을 1로 초기화
      callNum = 1;
    }
  }

  class User : Player
  {
    public User(string name) : base(name)
    {
    }
    // 배팅 옵션 선택
    public override int BattingSelect(Dealer dealer, int selectBattingOption)
    {
      // 현재 보유금과 배팅금에 따라 선택 불가능한 옵션이 있기 때문에 Money와 CurrentBettingMoney 전달
      OtherBattingOption selectedOption = Window.SelectMenu<OtherBattingOption>(Poker.DIALOGUE_CURSOR_Y + 3, Money, dealer.CurrentBettingMoney);
      selectBattingOption = (int)selectedOption;

      return selectBattingOption;
    }
    // 배팅 금액 설정
    public override void BattingMoney(Dealer dealer, ref int callNum)
    {
      int battingMoney;

      // 올바른 배팅금을 입력받을 때까지 반복
      while (true)
      {
        Console.WriteLine();
        Console.Write($"배팅금은 {dealer.CurrentBettingMoney}보다 크고 {Money}보다 작아야 합니다.");
        Console.Write($"배팅금을 입력하세요 : ");
        int.TryParse(Console.ReadLine(), out battingMoney);
        // 배팅금 범위 벗어나면 재입력
        if (battingMoney < dealer.CurrentBettingMoney || battingMoney > Money)
        {
          continue;
        }
        // 잘 입력 했으면 입력 종료
        else
        {
          break;
        }
      }
      // 입력한 배팅금으로 배팅 진행
      base.SetBattingMoney(dealer, ref callNum, battingMoney);
      Poker.DialogueClear();
    }
  }

  class Computer : Player
  {
    #region 컴퓨터 배팅 옵션별 선택 확률 
    const int CALL_PROBABILITY = 80;
    const int HALF_PROBABILITY = 5;
    const int BATTING_PROBABILITY = 10;
    const int DIE_PROBABILITY = 5;
    #endregion

    public Computer(string name) : base(name)
    {
    }
    // 배팅 옵션 선택
    public override int BattingSelect(Dealer dealer, int selectBattingOption)
    {
      // 랜덤에 같은 시드 방지
      Thread.Sleep(1);
      Random random = new Random();

      selectBattingOption = random.Next(CALL_PROBABILITY+ HALF_PROBABILITY+ BATTING_PROBABILITY+ DIE_PROBABILITY);

      // 각 옵션 별로 선택될 확률 구현
      // 다이부터 판단
      if (selectBattingOption < DIE_PROBABILITY)
      {
        selectBattingOption = (int)OtherBattingOption.다이;
      }
      // 하프는
      else if (selectBattingOption < DIE_PROBABILITY + HALF_PROBABILITY &&
        // 배팅금이 충분 할때만 하프 진행
        Money > dealer.CurrentBettingMoney)
      {
        selectBattingOption = (int)OtherBattingOption.하프;
      }
      // 배팅은
      else if (selectBattingOption < DIE_PROBABILITY + HALF_PROBABILITY + BATTING_PROBABILITY && 
        // 배팅금이 최소 배팅금+현재 배팅금 보다 많을 때 진행
        Money > dealer.CurrentBettingMoney + MIN_BATTING_UNIT)
      {
        selectBattingOption = (int)OtherBattingOption.배팅;
      }
      // 나머지 경우 모두 콜
      else
      {
        selectBattingOption = (int)OtherBattingOption.콜;
      }

      Console.WriteLine($"{Name}이 {(OtherBattingOption)selectBattingOption}을(를) 선택했습니다.");
      return selectBattingOption;
    }
    // 배팅 금액 설정
    public override void BattingMoney(Dealer dealer, ref int callNum)
    {
      int battingMoney;
      Thread.Sleep(1);
      Random random = new Random();

      while (true)
      {
        // 최소배팅금도 없는데 배팅했으면 전재산 배팅
        if (Money <= MIN_BATTING_UNIT)
        {
          battingMoney = Money;
          break;
        }

        battingMoney = random.Next(
          // 배팅금은 현재 배팅금부터 현재 배팅금의 1.5배까지 설정하고
          dealer.CurrentBettingMoney, (int)(dealer.CurrentBettingMoney * 1.5f))

          // MIN_BATTING_UNIT으로 나눈 뒤 다시 곱해서, 최소 배팅 단위를 구현 (int형이라 소수점 버려지는 것 이용)
          / MIN_BATTING_UNIT * MIN_BATTING_UNIT + 
          // 현재 배팅금보다 최소 배팅단위 만큼은 크게 배팅하기 위해
          MIN_BATTING_UNIT;

        // 돈 부족하면 배팅금 다시 설정
        if (battingMoney > Money)
        {
          continue;
        }
        // 돈 충분하면 배팅 진행
        else
        {
          break;
        }
      }
      Console.WriteLine($"{Name}이 {battingMoney}를 배팅했습니다.");
      Console.ReadKey(true);
      base.SetBattingMoney(dealer, ref callNum, battingMoney);
    }
  }
}
