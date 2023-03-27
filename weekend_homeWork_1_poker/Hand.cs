using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace weekend_homeWork_1_poker
{
  // 플레이어가 손에 들고 있는 카드
  class Hand
  {
    public List<Card> Cards { get; private set; }

    public Hand()
    {
      Cards = new List<Card>();
    }

    // 핸드에 카드 추가
    public void AddCard(Card card)
    {
      Cards.Add(card);
    }

    // 모든 턴 종료후 핸드 비움
    public void ClearHand()
    {
      Cards.Clear();
    }
  }
}
