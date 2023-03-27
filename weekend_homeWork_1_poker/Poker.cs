using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace weekend_homeWork_1_poker
{
  #region 배팅 메뉴
  public enum FirstBattingOption
  {
    배팅,
    다이
  }
  public enum OtherBattingOption
  {
    배팅,
    콜,
    하프,
    다이
  }
  #endregion

  // 포커게임 전체를 관리하는 클래스
  class Poker
  {
    #region 변수들
    // 게임 관련 제한 사항들
    public const int DEFAULT_BET = 1000;  // 기본 배팅금
    const int MAX_CARD_NUM = 7;           // 플레이어가 들고 있을 수 있는 최대 카드 수
    const int SHUFFLE_SPEED = 500;        // 딜러가 덱을 섞는 속도
    const int DEAL_SPEED = 500;           // 딜러가 카드를 나눠주는 속도
    const int DIALOGUE_CURSOR_X = 1;      // 메세지 출력 X 위치
    public static int DIALOGUE_CURSOR_Y;  // 메세지 출력 Y 위치. 플레이어 수에 따라 위치가 바뀌므로 const 불가능

    // 플레이어 관련 변수들
    private int playerNum;                // 선택한 플레이어 수
    private static int bankruptPlayerNum; // 파산한 플레이어 수
    private int diePlayerNum;             // 다이를 외친 플레이어 수
    private int playerIndex { get; set; } // 몇번째 플레이어?
    private bool openAllHands;            // 모든 플레이어의 카드 오픈할지 여부
    public void SetPlayerIndex(int value)
    {
      if (value >= playerNum)
      {
        playerIndex = 0;
      }
      else
      {
        playerIndex = value;
      }
    } // 플레이어 인덱스가 플레이어 수를 못 넘어가게 제한

    // 플레이어들과 딜러
    private List<Player> players;
    private Dealer dealer;
    #endregion

    // 전체 게임 흐름
    public void Play()
    {
      Start();

      // 게임 시작
      while (true)
      {
        dealer = new Dealer();

        // 새게임 시작시 초기화 할 것들
        diePlayerNum = 0;
        openAllHands = false;
        // 플레이어 정보 표시 및 안내 메세지 삭제
        Console.ReadKey(true);
        DialogueClear();
        ShowStat();

        // 내가 파산했으면 게임 종료
        if (players[0].isBankrupt)
        {
          return;
        }

        // 첫번째 턴, 3장을 받음
        FirstTurn();
        // 4~7번째 턴, 1장씩 받고 배팅함
        for (int turn = 4; turn <= MAX_CARD_NUM; turn++)
        {
          // 한명만 살아 있으면 게임 종료
          if ((playerNum - diePlayerNum - bankruptPlayerNum).Equals(1))
          {
            break;
          }
          // 2명이상 살아 있으면 다음턴 진행
          OtherTurn(in turn);
          StartBatting();
        }

        #region 게임 종료
        Console.ReadKey(true);
        DialogueClear();
        ShowStat();
        Console.WriteLine("게임 종료");
        Console.WriteLine("모든 플레이어의 패를 공개합니다.");

        // 모든 카드를 오픈하고
        openAllHands = true;
        ShowStat();

        // 우승자가 누군지 판별하고
        Player winner = dealer.WinnerDetermine(players);
        // 우승자에게 배팅금 지급
        WinnerCheck();

        // 게임이 끝나고 파산한 사람을 체크하고, Die상태를 false로 돌려줌
        BankruptCheck();
        #endregion 게임 종료
      }

    }

    #region 시작 설정
    // 환영 메세지 출력, 플레이 인원 선택, 플레이어와 딜러 생성
    public void Start()
    {
      Console.Clear();
      Console.WriteLine("포커 게임을 시작합니다.");
      Console.WriteLine("플레이어 인원을 선택해 주세요.");
      Console.WriteLine();
      PokerPlayerNum selectedPlayerNum = Window.SelectMenu<PokerPlayerNum>(Console.CursorTop);
      Console.WriteLine();
      Console.WriteLine($"선택된 플레이어 수: {selectedPlayerNum}");
      // 선택된 enum을 int형으로 변환해 playerNum에 저장함
      playerNum = (int)selectedPlayerNum;

      CreatePlayer(playerNum);
    }
    // 플레이어들 생성
    public void CreatePlayer(int playerNum)
    {
      players = new List<Player>();
      players.Add(new User("Player 1"));

      for (int i = 1; i < playerNum; i++)
      {
        players.Add(new Computer($"Player {i + 1}"));
      }
      DIALOGUE_CURSOR_Y = 3 * (playerNum + 1) + 1;
    }
    #endregion

    #region 게임 진행
    #region 첫턴
    public void FirstTurn()
    {
      // 플레이어에게 각각 3장을 나눠줌
      int firstDealNum = 3;
      Console.ReadKey(true);
      DialogueClear();

      // 덱 셔플
      Console.Write("딜러가 카드를 셔플합니다.");
      for (int i = 0; i < 3; i++)
      {
        Thread.Sleep(SHUFFLE_SPEED);
        Console.Write(" .");
      }
      Console.WriteLine();
      dealer.ShuffleDeck();
      DialogueClear();

      // 카드 분배
      Console.Write("카드를 분배합니다.");
      for (int i = 0; i < firstDealNum; i++)
      {
        for (int playerIndex = 0; playerIndex < playerNum; playerIndex++)
        {
          Thread.Sleep(DEAL_SPEED);
          dealer.DealCard(players[playerIndex]);
          ShowStat();
        }
      }
      DialogueClear();

      // 기본 금액 차감
      Console.WriteLine($"기본 배팅 금액은 {DEFAULT_BET}원 입니다.");
      for (int playerIndex = 0; playerIndex < playerNum; playerIndex++)
      {
        players[playerIndex].SetMoney(players[playerIndex].Money - DEFAULT_BET);
        dealer.TotalBettingMoney += DEFAULT_BET;
      }
      ShowStat();
    }
    #endregion 첫턴
    #region 4~7턴 분배
    public void OtherTurn(in int turn)
    {
      Console.ReadKey(true);
      DialogueClear();
      Console.WriteLine($"{turn}번째 카드를 분배합니다.");
      for (int playerIndex = 0; playerIndex < playerNum; playerIndex++)
      {
        // 다이거나 파산한 플레이어는 건너 뜀
        if (players[playerIndex].isDie || players[playerIndex].isBankrupt)
        {
          continue;
        }
        dealer.DealCard(players[playerIndex]);
        Thread.Sleep(DEAL_SPEED);
        ShowStat();
      }
      ShowStat();
    }
    #endregion 4~7턴 분배
    #region 4~7턴 배팅
    public void StartBatting()
    {
      int callNum = 0;
      int selectBattingOption = 0;
      bool isBattingFirst = true;

      // 이미 올인한 플레이어가 있으면 배팅 스킵
      for (int playerIndex = 0; playerIndex < playerNum; playerIndex++)
      {
        if (players[playerIndex].Money < DEFAULT_BET && !players[playerIndex].isBankrupt)
        {
          return;
        }
      }

      Console.ReadKey(true);
      DialogueClear();
      Console.WriteLine("배팅을 시작합니다.");
      dealer.CurrentBettingMoney = DEFAULT_BET;
      Console.WriteLine($"배팅은 {players[0].Name}부터 시작합니다.");
      SetPlayerIndex(0);

      // 모든 플레이어의 이전 배팅 금액을 0으로 초기화
      for (int playerIndex = 0; playerIndex < playerNum; playerIndex++)
      {
        players[playerIndex].PreviousBettingMoney = 0;
      }

      // 첫턴 플레이어부터 배팅 시작
      // 내가 다이를 안 했으면
      if (!players[0].isDie)
      {
        Console.ReadKey(true);
        // 첫 배팅 옵션을 출력해서 선택함
        FirstBattingOption selectedOption = Window.SelectMenu<FirstBattingOption>(DIALOGUE_CURSOR_Y + 3 - 3 * bankruptPlayerNum);
        // 배팅 선택 했으면 
        if (selectedOption.Equals(FirstBattingOption.배팅))
        {
          // 배팅 진행하고
          players[0].BattingMoney(dealer, ref callNum);
          // 콘솔에 배팅 현황 갱신하고
          ShowStat();
          // 첫 배팅 끝났음을 체크
          isBattingFirst = false;
        }
        // 다이를 선택 했으면
        else if (selectedOption.Equals(FirstBattingOption.다이))
        {
          // 다이 여부를 true 시키고
          players[0].isDie = true;
          // 다이한 플레이어수를 늘려줌
          diePlayerNum++;
          // 배팅을 안 했기 때문에 isBattingFirst는 여전히 true
        }
      }
      // 내 배팅 끝났으니까 다음 플레이어로 넘어감
      SetPlayerIndex(playerIndex + 1);
      DialogueClear();

      // 모든 플레이어가 다이 or 콜을 할때까지 배팅이 반복됨
      while (true)
      {
        // 모두 콜 했으면 배팅 종료
        if (callNum.Equals(playerNum - bankruptPlayerNum - diePlayerNum))
        {
          break;
        }
        // 한명 빼고 다 다이했으면 남은 플레이어가 우승
        else if ((playerNum - bankruptPlayerNum - diePlayerNum).Equals(1))
        {
          Console.WriteLine("모든 플레이어가 죽었습니다.");
          Console.ReadKey(true);
          players[playerIndex].isWin = true;
          break;
        }
        // 플레이어가 다이거나 파산했으면 건너 뜀
        if (players[playerIndex].isDie || players[playerIndex].isBankrupt)
        {
          SetPlayerIndex(playerIndex + 1);
          continue;
        }

        // 배팅 옵션 선택
        // 플레이어가 죽어서 컴퓨터가 첫 배팅이면 무조건 배팅하게 설정
        if (isBattingFirst)
        {
          selectBattingOption = 0;
          isBattingFirst = false; Console.WriteLine($"{players[playerIndex].Name}이 {(FirstBattingOption)0}을(를) 선택했습니다.");
        }
        // 아니면 콜, 하프, 다이, 배팅 중에 선택
        else
        {
          selectBattingOption = players[playerIndex].BattingSelect(dealer, selectBattingOption);
        }
        if (playerIndex != 0)
        {
          Console.ReadKey(true);
        }
        DialogueClear();

        // 선택한 배팅 옵션에 맞게 메서드 실행
        switch (selectBattingOption)
        {
          case (int)OtherBattingOption.배팅:
            players[playerIndex].BattingMoney(dealer, ref callNum);
            break;
          case (int)OtherBattingOption.다이:
            players[playerIndex].isDie = true;
            diePlayerNum++;
            break;
          case (int)OtherBattingOption.하프:
            players[playerIndex].Half(dealer, ref callNum);
            break;
          case (int)OtherBattingOption.콜:
            players[playerIndex].Call(dealer, ref callNum);
            break;
        }
        // 배팅 끝났으면 다음 플레이어로 넘어감
        SetPlayerIndex(playerIndex + 1);
        ShowStat();
      }

      // 현재 턴의 배팅 끝났으면 현재 배팅금 초기화
      dealer.CurrentBettingMoney = 0;
      ShowStat();
    }
    #endregion 4~7턴 배팅
    #endregion 게임 진행

    #region 게임 종료
    // 우승자에게 배팅금 지급
    public void WinnerCheck()
    {
      for (int playerIndex = 0; playerIndex < playerNum; playerIndex++)
      {
        // 우승자면
        if (players[playerIndex].isWin)
        {
          // 총 배팅금액을 소지금에 추가해주고
          players[playerIndex].SetMoney(players[playerIndex].Money + dealer.TotalBettingMoney);
          // 배팅금액들 초기화하고
          dealer.TotalBettingMoney = 0;
          dealer.CurrentBettingMoney = 0;
          DialogueClear();
          // 누가 우승했는지 알려줌
          Console.WriteLine($"{players[playerIndex].Name} 승리!");
          return;
        }
      }
    }
    // 파산한 사람 있는지 체크하고 모든 세팅 초기화
    public void BankruptCheck()
    {
      // 파산한 플레이어 여부 판단
      // 내가 파산하면 파산 메세지 출력
      if (players[0].Money < DEFAULT_BET)
      {
        Console.WriteLine("파산하였습니다...");
        players[0].isBankrupt = true;
        Console.ReadKey(true);
      }
      // 파산 안 했으면 다이랑 윈 false로 초기화하고 패를 버림
      else
      {
        players[0].isDie = false;
        players[0].isWin = false;
        players[0].Hand.ClearHand();
      }

      // 다른 플레이어들
      for (int playerIndex = 1; playerIndex < playerNum; playerIndex++)
      {
        // 아직 파산 안했는데 이번판에 돈을 다 썻으면
        if (players[playerIndex].Money < DEFAULT_BET && !players[playerIndex].isBankrupt)
        {
          // 파산 체크하고 파산 인원 증가
          players[playerIndex].isBankrupt = true;
          bankruptPlayerNum++;
        }
        // 이미 파산 했었으면
        else if (players[playerIndex].isBankrupt)
        {
          // 넘어감
          continue;
        }
        // 파산 아직 안했으면
        else
        {
          // 세팅 초기화
          players[playerIndex].isDie = false;
          players[playerIndex].isWin = false;
          players[playerIndex].Hand.ClearHand();
        }
      }
    }

    #endregion 게임 종료

    #region 콘솔 창 띄우기
    // 총 배팅금, 플레이어+소지금, 플레이어 카드
    public void ShowStat()
    {
      // 원래 표시되던 것들 지우고
      Console.SetCursorPosition(0, 0);
      for (int i = 0; i < DIALOGUE_CURSOR_Y - 3 * bankruptPlayerNum; i++)
      {
        Console.WriteLine(new string(' ', Window.WINDOW_WIDTH - 1));
      }
      // 새롭게 출력
      Console.SetCursorPosition(0, 0);

      // 딜러가 관리하는 배팅금액
      Console.WriteLine($"총 배팅 금액 : {dealer.TotalBettingMoney}     현재 배팅 금액 : {dealer.CurrentBettingMoney}");
      Console.WriteLine();

      // 플레이어 정보 출력 반복문
      for (int playerIndex = 0; playerIndex < playerNum; playerIndex++)
      {
        // 파산한 플레이어는 표시하지 않고 건너 뜀
        if (players[playerIndex].isBankrupt)
        {
          continue;
        }

        #region 플레이어 이름, 소지금, 보유 카드 표시
        Console.WriteLine($"{players[playerIndex].Name} (소지금 : {players[playerIndex].Money})");
        // 플레이어 이름 밑에 소유한 패 표시
        for (int cardIndex = 0; cardIndex < players[playerIndex].Hand.Cards.Count; cardIndex++)
        {
          // 카드 문양과 숫자에 맞는 표시로 변환
          string card = Card.GetCardDisplayString(players[playerIndex].Hand.Cards[cardIndex]);

          // 본인 카드이거나 최종 결과 출력일 시 모든 카드 표시
          if (playerIndex.Equals(0) || openAllHands)
          {
            // 카드 번호가 10이면 한칸 길기 때문에 정렬을 위해
            // 번호 길이에 맞게 오른쪽 공백 추가
            Console.Write($"|  {card}" + new string(' ', 5 - card.Length) + "|");
          }
          // 다른 플레이어 카드는 1,2,7번 카드 hidden으로 표시
          else
          {
            switch (cardIndex)
            {
              case 0:
              case 1:
              case MAX_CARD_NUM - 1:
                Console.Write($"| Hidden |");
                break;
              // 1, 2, 7번 제외하고는 오픈
              default:
                Console.Write($"|  {card}" + new string(' ', 5 - card.Length) + "|");
                break;
            }
          }
        }
        #endregion 플레이어 이름, 소지금, 보유 카드

        #region 게임이 끝나고 패를 공개했을 때 완성한 족보 표시
        Console.WriteLine();
        if (openAllHands)
        {
          // 플레이어 족보 확인
          HandRank handResult = dealer.EvaluateHand(players[playerIndex]);
          // 보유 족보 중 가장 높은 족보 출력
          string suitStr = "";
          string rankStr = "";

          // 문양 설정
          switch (players[playerIndex].highSuit)
          {
            case (int)Suit.Clubs:
              suitStr = "♣";
              break;
            case (int)Suit.Hearts:
              suitStr = "♥";
              break;
            case (int)Suit.Diamonds:
              suitStr = "◆";
              break;
            case (int)Suit.Spades:
              suitStr = "♠";
              break;
          }

          // 숫자 설정
          // 2~10은 그냥 숫자 출력
          if (players[playerIndex].highRank >= (int)Rank.Two && players[playerIndex].highRank <= (int)Rank.Ten)
          {
            rankStr = players[playerIndex].highRank.ToString();
          }
          // A,J,Q,K는 알파벳 출력
          else
          {
            switch (players[playerIndex].highRank)
            {
              case (int)Rank.Jack:
                rankStr = "J";
                break;
              case (int)Rank.Queen:
                rankStr = "Q";
                break;
              case (int)Rank.King:
                rankStr = "K";
                break;
              case (int)Rank.Ace:
                rankStr = "A";
                break;
            }
          }
          Console.WriteLine($"완성한 패 : {handResult} {suitStr}{rankStr}");

          string displayStr = $"{suitStr}{rankStr}";
        }
        // 패를 공개하지 않을 땐 공백 출력
        else
        {
          Console.WriteLine();
        }
        #endregion 족보 표시
      }
      Console.WriteLine("ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ");
      SetCursorDialogue();
    }

    #region 안내 메세지
    static public void SetCursorDialogue()
    {
      Console.SetCursorPosition(0, DIALOGUE_CURSOR_Y - 3 * bankruptPlayerNum);
    }

    static public void DialogueClear()
    {
      SetCursorDialogue();
      for (int i = 0; i < Window.WINDOW_HEIGHT - DIALOGUE_CURSOR_Y - 1 + 3 * bankruptPlayerNum; i++)
      {
        Console.WriteLine(new string(' ', Window.WINDOW_WIDTH - 1));
      }
      SetCursorDialogue();
    }
    #endregion 안내 메세지

    #endregion 콘솔 창 띄우기
  }
}
