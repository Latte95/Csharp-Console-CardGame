using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace weekend_homeWork_1_poker
{
  // 제대로 나오는지 확인 못한 족보 : 스티플, 포카드, 6장 연속
  // 그외 나머지 족보는 정상 작동 확인함
  // 구현 안한 족보 : 마운틴
  // 로티플, 백티플, 백스트레이트는 스티플과 스트레이트로 충분히 판별 가능해서 구현 x
  // A가 중간에 있는 스트레이트는 족보 사이트를 늦게 봐서 스트레이트로 판별되게 구현하였음...

  // 버그 발생하는지 확인한 것들
  // 1. 다이인 플레이어 패가 가장 높을 때 우승하진 않는지 => 다이인 플레이어는 패 비교할 때 무시함
  // 2. 소지금이 없는데 배팅 옵션을 선택한 경우 => 배팅을 못해서 무한루프에 빠짐
  //                                  진입 못하게 설정해두고 혹시 진입했더라도 전재산만 배팅하게 설정
  // 3. 한명 빼고 모두 다이일 때 => 남은 1명이 우승함
  // 4. 소지금이 불충분 할 때 하프를 선택하면 => 전재산 배팅, 배팅액도 그에 맞는 금액만 증가
  // 5. 파산한 플레이어가 있을 때 => 메세지 위치, 메뉴 위치 정상 출력되게 커서위치 설정함
  // 6. 풀하우스 vs 풀하우스 => 3장쪽 숫자로 판단함

  class Program
  {
    static void Main(string[] args)
    {
      // 콘솔 윈도우 세팅
      Window.PreferencesWindow();

      // 메인 화면 출력 및 메뉴 선택
      MainMenu selectedMenuItem = Window.MainScreen();
      // 선택한 메뉴 실행
      switch (selectedMenuItem)
      {
        case MainMenu.포커:
          Poker pokerGame = new Poker();
          pokerGame.Play();
          break;
        case MainMenu.월남뽕:
          WalNamPong walNamPong = new WalNamPong();
          walNamPong.Play();
          break;
        case MainMenu.게임종료:
          Console.WriteLine("게임을 종료합니다.");
          break;
        default:
          Console.WriteLine("메뉴 스위치문 추가 안했다");
          break;
      }

      Console.ReadKey(true);
    }

    // 한글은 길이2 영어는 길이1이기 때문에 글자의 길이에 맞게 문자열의 길이를 반환하는 함수
    static public int GetStringLength(string str)
    {
      int length = 0; // 문자열의 길이를 저장할 변수

      // 문자열의 길이만큼 반복하는데
      // c에 str문자열의 첫 글자부터 순서대로 대입함
      // 이 c의 문자의 길이가 2인지 1인지 판단해서 문자열의 길이를 구함
      // System.Globalization.UnicodeCategory.OtherLetter는 문자 하나가 2의 길이를 갖는 문자를 뜻함
      foreach (char c in str)
      {
        // 문자가 길이를 2를 갖는다면 길이 2증가
        if (char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherLetter)
        {
          length += 2;
        }
        // 문자열의 길이가 2가 아니면 길이 1증가
        else
        {
          length++;
        }
      }
      return length;
    }

  }
}
