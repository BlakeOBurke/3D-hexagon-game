using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Threading;

namespace project
{
    enum damageType
    {
        Bomb,
        Gun,
        Missile,
        Shell
    }
    enum armourType
    {
        Heavy,
        Light,
        Immobile
        //bomb -> land/water
        //missile -> air
        //gun -> light
        //shell -> heavy

        //bomb -x> air
        //gun -x> heavy
        //shell -x> air
    }
    enum Domain
    {
        Air,
        Water,
        Land,
        //air -> go anywhere
        //land -> go on land
        //water -> go on water
    }
    public 
        enum Unit
    {
        Base,//heavy 
        Tank,
        AA_Gun,
        Bomber,
        Fighter,
        Aircraft_Carrier,
        Missile_Boat
    }


    interface Ientity
    {
        Game.Shape shape { get; set; }
        (int,int) position {get; set;}
        bool canAttack { get; set; }
        Unit type { get; set; }
        Domain domain { get; set; }
        armourType armour { get; set; }
        damageType gunType { get; set; }
        int moveSpeed { get; set; }
        int range { get; set; }
        int team { get; set; }
        int health { get; set;}
        int damage { get; set; }

        void Damage(Ientity target);
        void TakeDamage(int damage,damageType Damage);
        bool IsLive();
        void Kill();
        void Display();
        void Move((int,int) newPos);
        List<(int, int)> GetMoves();
    }


    class Base : Ientity
    {
        public Game.Shape shape { get; set; }
        public (int, int) position { get; set; }
        public bool canAttack { get; set; } = false;
        public Unit type { get; set; } = Unit.Base;
        public Domain domain { get; set; } = Domain.Land;
        public armourType armour { get; set; } = armourType.Immobile;
        public damageType gunType { get; set; } = damageType.Gun;
        public int moveSpeed { get; set; } = 0;
        public int range { get; set; } = 0;
        public int team { get; set; }

        public int health { get; set; }
        public int damage { get; set; }

        public void Damage(Ientity target)
        {
            return;
        }
        public void TakeDamage(int damage, damageType Damage)
        {
            health -= damage;
            if (!IsLive())
            {
                Kill();
            }
        }
        public bool IsLive()
        {
            if(health <= 0)
            {
                return false;
            }
            return true;
        }
        public void Kill()
        {
            throw new NotImplementedException();
        }
        public void Display()
        {
            //set centre = this
            Game.Tile A = Game.Tile.Hexagons[position];
            Vector3 entityPos = 8 * new Vector3((A.qr.Item1 + A.qr.Item2 / 2f) * 2, -1, (float)Math.Sqrt(3) * (A.qr.Item2)) + new Vector3(0, 3 * A.height, 0);
            return;
        }
        public void Move((int, int) newPos)
        {
            return;
        }
        public List<(int,int)>  GetMoves()
        {
            return null;
        }
        public Base(Game.Shape shape, (int, int) position, int team, int health, int damage)
        {
            this.shape = shape;
            this.position = position;
            this.team = team;
            this.health = health;
            this.damage = damage;
        }
    }
    class Tank : Ientity
    {
        public Game.Shape shape { get; set; } 
        public (int, int) position { get; set; }
        public bool canAttack { get; set; } = true;
        public Unit type { get; set; } = Unit.Tank;
        public Domain domain { get; set; } = Domain.Land;
        public armourType armour { get; set; } = armourType.Heavy;
        public damageType gunType { get; set; } = damageType.Shell;
        public int moveSpeed { get; set; } = 3;
        public int range { get; set; } = 3;
        public int team { get; set; }

        public int health { get; set; } = 0;
        public int damage { get; set; }

        public void Damage(Ientity target)
        {
            target.TakeDamage(damage, this.gunType);
        }
        public void TakeDamage(int damage, damageType Damage)
        {
            if(Damage == damageType.Shell || Damage == damageType.Bomb)
            {
                damage =(int)(damage*3/2);
            }
            else if(Damage == damageType.Gun)
            {
                damage =(int)(damage*1/2);
            }
            health -= damage;
            if (!IsLive())
            {
                Kill();
            }
        }
        public bool IsLive()
        {
            if (health <= 0)
            {
                return false;
            }
            return true;
        }
        public void Kill()
        {
            throw new NotImplementedException();
        }
        public void Display()
        {
            //set centre = this
            Game.Tile A = Game.Tile.Hexagons[position];
            shape.centre = 8 * new Vector3((A.qr.Item1 + A.qr.Item2 / 2f) * 2, -1, (float)Math.Sqrt(3) * (A.qr.Item2)) + new Vector3(0, 3 * A.height+30, 0);
            return;
        }
        public void Move((int,int) newPos)
        {
            position = newPos;
        }
        public List<(int, int)> GetMoves()
        {



            try
            {
                //return Hex.RangeN(position, moveSpeed).Where(x => Game.Tile.Hexagons.Where(y => y.type == Game.pieces.Land).ToList().Contains()).ToList();
                List<(int, int)> MOVES = Hex.RangeN(position, moveSpeed);
                List<(int, int)> validMoves = new List<(int, int)>();
                for (int i = 0; i < MOVES.Count(); i++)
                {
                    try
                    {
                        Game.Tile QR = Game.Tile.Hexagons[MOVES[i]];
                        if (QR.type == Game.pieces.Land || QR.type == Game.pieces.Shore)
                        {
                            validMoves.Add(QR.qr);
                        }
                    }
                    catch { }
                }
                return validMoves;
            }
            catch
            {
                return null;
            }
        }
        public Tank((int, int) position, int team = 0, int health = 100, int damage = 10)
        {
            this.shape = new Game.Shape("cube.obj", team == 0 ? Color.Blue : Color.DarkRed);
            this.shape.scale *= 7;
            this.position = (0, 0);
            this.team = team;
            this.health = health;
            this.damage = damage;
        }
    }


    class Manager
    {
        public static List<Ientity> pieces = new List<Ientity>();

        public static void setup()
        {
            //start stuff
            Tank A = new Tank((0,0),1);
            Tank B = new Tank((0,0),0);

            pieces.Add(A);
            pieces.Add(new Tank((0,0),1));
            pieces.Add(B);
        }
        public static int currTeam = 0;
        public static void Turn()
        {
            List<Ientity> list = pieces.Where(x=>x.team == currTeam).ToList();
            //place pieces

            //move
            for (int i = 0; i < list.Count; i++)
            {
                //try move


                //get moves
                List<(int, int)> moves = list[i].GetMoves();

                Game.DisplayMoves = moves.ToList();

                while (true)
                {
                    if(moves.Contains(Game.curHex) && Game.currentKey.IsKeyDown(Key.Enter))
                    {
                        list[i].Move(Game.curHex);
                        break;
                    }
                }


                //attack

                while (true)
                {
                    //
                    break;
                }
            }

            Thread.Sleep(100);
            Game.DoNextTurn(1 - currTeam);
            return;
        }
    }

    internal class entity
    {

    }
}
