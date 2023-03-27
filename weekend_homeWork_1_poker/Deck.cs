using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace weekend_homeWork_1_poker
{
  // 카드 클래스를 통해 덱을 만들고, 플레이어에게 분배된 카드는 삭제되는 클래스
  class Deck
  {
    const int SHUFFLE_COUNT = 1000;

    private List<Card> cards;
    public int Size { get { return cards.Count; } } // 덱에 있는 카드 수

    public Deck()
    {
      MakeDeck();
    }

    // 덱 생성
    public void MakeDeck()
    {
      cards = new List<Card>();
      // 카드 문양과 숫자만큼 반복해서 모든 카드를 덱에 넣음
      foreach (Suit suit in Enum.GetValues(typeof(Suit)))
      {
        foreach (Rank rank in Enum.GetValues(typeof(Rank)))
        {
          cards.Add(new Card(suit, rank));
        }
      }
      // 덱 완성 후 카드 셔플
      Shuffle();
    }

    public void Shuffle()
    {
      Thread.Sleep(1);
      Random random = new Random();
      int dest;
      int sour;
      Card temp;

      // 셔플 진행
      for(int i = 0; i< SHUFFLE_COUNT; i++)
      {
        dest = random.Next(cards.Count);
        sour = random.Next(cards.Count);

        temp = cards[dest];
        cards[dest] = cards[sour];
        cards[sour] = temp;
      }
    }

    public Card DrawCard()
    {
      // 덱에 카드가 없다면 새로운 덱을 생성
      if (cards.Count == 0)
      {
        Console.WriteLine("카드를 모두 소진하였습니다.");
        Console.WriteLine("새로운 덱을 가져옵니다.");
        Console.ReadKey(true);
        Poker.DialogueClear();
        MakeDeck();
      }
      // 덱에 카드가 있으면 카드 한장을 뽑은뒤
      Card drawnCard = cards.First();
      // 뽑은 카드를 덱에서 삭제하고
      cards.RemoveAt(0);
      // 뽑은 카드를 반환함
      return drawnCard;
    }
  }
}
