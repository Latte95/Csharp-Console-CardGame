using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace weekend_homeWork_1_poker
{
  // 메인 메뉴 옵션들
  enum MainMenu
  {
    포커,
    월남뽕,
    게임종료
  }
  // 플레이 인원 선택
  enum PokerPlayerNum
  {
    두명 = 2,
    세명,
    네명
  }

  // 콘솔 화면 띄우는 클래스
  class Window
  {
    const string TITLE_NAME = "경일 게임장";
    public const int WINDOW_WIDTH = 120;
    public const int WINDOW_HEIGHT = 30;
    const int MENU_CURSOR_X = 2;

    // 콘솔창 기본 설정
    public static void PreferencesWindow()
    {
      Console.Title = TITLE_NAME;
      Console.WindowWidth = WINDOW_WIDTH;
      Console.WindowHeight = WINDOW_HEIGHT;
      Console.CursorVisible = false;
    }

    // 첫 화면 출력
    public static MainMenu MainScreen()
    {
      Console.WriteLine($"{TITLE_NAME}에 오신 걸 환영합니다.");
      Console.WriteLine();

      // 메인화면에서 선택 가능한 메뉴들 출력 및 선택하는 함수
      MainMenu selectedMenuItem = SelectMenu<MainMenu>(Console.CursorTop);

      return selectedMenuItem;
    }

    // 다양한 enum을 받기 위해 타입 매개 변수 T를 이용하였고, 이를 where을 통해 Enum으로 한정시켰음
    public static T SelectMenu<T>(int startY) where T : Enum
    {
      return SelectMenu<T>(startY, 0, 0);
    }
    public static T SelectMenu<T>(int startY, int playerMoney, int dealerCurrentBettingMoney) where T : Enum
    {
      // 선택 포인터(▶) 위치를 제한하기 위한 변수
      int pointerPositionY = 0;
      // 선택 가능한 메뉴 수
      int menuLength = Enum.GetValues(typeof(T)).Length;

      // 메뉴 출력
      PrintMenu<T>(pointerPositionY, startY);

      // 키 입력에 따라 ▶ 위치를 바꿔주는 반복문
      while (true)
      {
        // 사용자가 입력한 키를 저장하는 변수
        ConsoleKey key = Console.ReadKey(true).Key;
        // ▶ 위치를 갱신하기 위한 이전 ▶ 위치, 사용자가 키입력을 하기 전의 위치를 저장
        int oldPosition = pointerPositionY;
        // 입력한 키에 따른 행동을 정하는 조건문
        switch (key)
        {
          // ▶이동
          // ▶가 메뉴 밖으로 나가지 않게 하기 위한 나머지 연산
          case ConsoleKey.UpArrow:
            // 처음 위치가 0이기 때문에 menuLength를 한번 더해줌
            pointerPositionY = (pointerPositionY - 1 + menuLength) % menuLength;
            break;
          case ConsoleKey.DownArrow:
            pointerPositionY = (pointerPositionY + 1) % menuLength;
            break;

          // 선택
          // 엔터 또는 스페이스바를 눌렀을 때 메뉴가 선택됨
          case ConsoleKey.Enter:
          case ConsoleKey.Spacebar:
            // 선택 불가 조건문
            // 여기서는 포커 진행중 돈이 없는데 배팅이나 하프를 시도하지 못하도록 제한함
            if (typeof(T) == typeof(OtherBattingOption) &&
              // 배팅할 돈이 없는데
              playerMoney < dealerCurrentBettingMoney + Poker.DEFAULT_BET &&
              // 배팅을 선택했으면?
              (OtherBattingOption)pointerPositionY == OtherBattingOption.배팅)
            {
              // 선택을 무시함
              continue;
            }
            else if (typeof(T) == typeof(OtherBattingOption) &&
              // 보유금보다 많은 배팅이 걸려있는데
              playerMoney < dealerCurrentBettingMoney &&
              // 하프를 선택하면?
              (OtherBattingOption)pointerPositionY == OtherBattingOption.하프)
            {
              // 선택을 무시함
              continue;
            }
            // 위의 조건에 걸리지 않는다면 
            // GetValues(typeof(T)) 전달받은 메뉴 목록중에
            // GetValue(pointerPositionY) ▶위치의 메뉴를 반환함
            return (T)Enum.GetValues(typeof(T)).GetValue(pointerPositionY);
        }

        // ▶ 위치가 바뀌었다면 ▶ 위치를 갱신함
        if (pointerPositionY != oldPosition)
        {
          // ▶ 위치만을 바꾸기 위한 메서드
          UpdatePointer(pointerPositionY, oldPosition, startY);
          // ▶ 위치를 바꾼 뒤에 입출력을 위해 커서 위치를 다시 메뉴 선택 밑으로 옮김
          Console.SetCursorPosition(0, startY + menuLength + 1);
        }
      }
    }

    static public void PrintMenu<T>(int pointerPositionY, int startY) where T : Enum
    {
      // 메뉴 이름을 전부 담는 배열
      string[] menuNames = Enum.GetNames(typeof(T));

      // 메뉴 수만큼 반복
      for (int i = 0; i < menuNames.Length; i++)
      {
        Console.SetCursorPosition(MENU_CURSOR_X, startY + i);
        // 현재 커서 위치라면 선택 포인터 ▶ 표시
        if (i == pointerPositionY)
        {
          Console.Write("▶ ");
        }
        // 그 외에는 ▶ 를 지움
        else
        {
          Console.Write("   ");
        }
        // 포인터 옆에 메뉴 이름 표시
        Console.WriteLine(menuNames[i]);
      }
    }

    static void UpdatePointer(int newPosition, int oldPosition, int startY)
    {
      // 콘솔 클리어 이용시 화면이 깜박거리기 때문에 커서만 새로 그려주도록 구현함
      // 원래 ▶가 있던 위치를 공백을 통해 지우고
      Console.SetCursorPosition(MENU_CURSOR_X, oldPosition + startY);
      Console.Write("   ");
      // 옮겨진 위치에 ▶ 표시
      Console.SetCursorPosition(MENU_CURSOR_X, newPosition + startY);
      Console.Write("▶ ");
    }

  }
}
