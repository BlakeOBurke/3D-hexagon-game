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
using System.CodeDom;
using System.Runtime.InteropServices;

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
        Missile_Boat,
        Null
    }


    interface Ientity
    {
        Unit SpecialType { get; set; }
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
        void attack((int, int) position);
        void Damage(Ientity target);
        void TakeDamage(int damage,damageType Damage);
        bool IsLive();
        void Kill();
        void Display();
        void Move((int,int) newPos);
        List<(int, int)> GetMoves();
        List<(int, int)> GetAttackable();
    }


    class Base : Ientity
    {
        public Unit SpecialType { get; set; }
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

        public void attack((int, int) position)
        {
            return;
        }

        public void Damage(Ientity target)
        {
            return;
        }
        public void TakeDamage(int damage, damageType Damage)
        {
            health -= damage;

            Console.WriteLine($"{this.type} took {damage} damage, {this.type} health = {health}");

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
            Console.WriteLine($"{this.type} was destroyed");
            Manager.pieces.Remove(this);
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
        public Base((int, int) position, int team=0, int health = 0, int damage = 0)
        {
            this.shape = new Game.Shape("cube.obj", team == 0 ? Color.Blue : Color.DarkRed);
            this.position = position;
            this.team = team;
            this.health = health;
            this.damage = damage;
        }
        public List<(int, int)> GetAttackable()
        {
            return null;
        }
    }
    class Tank : Ientity
    {
        public Unit SpecialType { get; set; }
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

        public void attack((int,int) position)
        {
            for (int i = 0; i < Manager.pieces.Count(); i++)
            {
                if(position == Manager.pieces[i].position && Manager.pieces[i].team != this.team)
                {
                    Damage(Manager.pieces[i]);
                }
            }
        }
        public void Damage(Ientity target)
        {
            target.TakeDamage(damage, this.gunType);
        }
        public void TakeDamage(int damage, damageType Damage)
        {
            if(Damage == damageType.Bomb || Damage == damageType.Bomb)
            {
                damage =(int)(damage*3/2);
            }
            else if(Damage == damageType.Gun)
            {
                damage =(int)(damage*1/2);
            }
            health -= damage;

            Console.WriteLine($"{this.type} took {damage} damage, {this.type} health = {health}");

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
            Console.WriteLine($"{this.type} was destroyed");
            Manager.pieces.Remove(this);
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
            Manager.Move(this, newPos);
        }
        public List<(int, int)> GetMoves()
        {
            return Manager.getMoves(this);
        }
        public List<(int, int)> GetAttackable()
        {
            return Manager.GetAttackable(this);
        }
        public Tank((int, int) position, int team = 0, int health = 60, int damage = 10)
        {
            this.shape = new Game.Shape("cube.obj", team == 0 ? Color.Blue : Color.DarkRed);
            this.shape.scale *= 7;
            this.position = position;
            this.team = team;
            this.health = health;
            this.damage = damage;
        }
    }
    class AA_Gun : Ientity
    {
        public Unit SpecialType { get; set; }
        public Game.Shape shape { get; set; }
        public (int, int) position { get; set; }
        public bool canAttack { get; set; } = true;
        public Unit type { get; set; } = Unit.AA_Gun;
        public Domain domain { get; set; } = Domain.Land;
        public armourType armour { get; set; } = armourType.Light;
        public damageType gunType { get; set; } = damageType.Gun;
        public int moveSpeed { get; set; } = 2;
        public int range { get; set; } = 4;
        public int team { get; set; }

        public int health { get; set; } = 0;
        public int damage { get; set; }

        public void attack((int, int) position)
        {
            for (int i = 0; i < Manager.pieces.Count(); i++)
            {
                if (position == Manager.pieces[i].position && Manager.pieces[i].team != this.team)
                {
                    Damage(Manager.pieces[i]);
                }
            }
        }
        public void Damage(Ientity target)
        {
            target.TakeDamage(damage, this.gunType);
        }
        public void TakeDamage(int damage, damageType Damage)
        {
            if (Damage == damageType.Gun || Damage == damageType.Bomb)
            {
                damage = (int)(damage * 3 / 2);
            }
            health -= damage;

            Console.WriteLine($"{this.type} took {damage} damage, {this.type} health = {health}");

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
            Console.WriteLine($"{this.type} was destroyed");
            Manager.pieces.Remove(this);
        }
        public void Display()
        {
            //set centre = this
            Game.Tile A = Game.Tile.Hexagons[position];
            shape.centre = 8 * new Vector3((A.qr.Item1 + A.qr.Item2 / 2f) * 2, -1, (float)Math.Sqrt(3) * (A.qr.Item2)) + new Vector3(0, 3 * A.height + 30, 0);
            return;
        }
        public void Move((int, int) newPos)
        {
            Manager.Move(this, newPos);
        }
        public List<(int, int)> GetMoves()
        {
            return Manager.getMoves(this);
        }

        public List<(int, int)> GetAttackable()
        {
            return Manager.GetAttackable(this);
        }
        public AA_Gun((int, int) position, int team = 0, int health = 40, int damage = 10)
        {
            this.shape = new Game.Shape("cube.obj", team == 0 ? Color.DarkBlue : Color.Red);
            this.shape.scale *= 7;
            this.shape.scale *= new Vector3(0.8f, 1.2f, 0.8f);
            this.position = position;
            this.team = team;
            this.health = health;
            this.damage = damage;
        }
    }
    class Fighter : Ientity
    {
        public Unit SpecialType { get; set; }
        public Game.Shape shape { get; set; }
        public (int, int) position { get; set; }
        public bool canAttack { get; set; } = true;
        public Unit type { get; set; } = Unit.Fighter;
        public Domain domain { get; set; } = Domain.Air;
        public armourType armour { get; set; } = armourType.Light;
        public damageType gunType { get; set; } = damageType.Gun;
        public int moveSpeed { get; set; } = 5;
        public int range { get; set; } = 2;
        public int team { get; set; }

        public int health { get; set; } = 0;
        public int damage { get; set; }

        public void attack((int, int) position)
        {
            for (int i = 0; i < Manager.pieces.Count(); i++)
            {
                if (position == Manager.pieces[i].position && Manager.pieces[i].team != this.team)
                {
                    Damage(Manager.pieces[i]);
                }
            }
        }
        public void Damage(Ientity target)
        {
            target.TakeDamage(damage, this.gunType);
        }
        public void TakeDamage(int damage, damageType Damage)
        {
            if (Damage == damageType.Missile)
            {
                damage = (int)(damage * 2);
            }
            if(Damage == damageType.Gun)
            {
                damage = (int)(damage * 3/2);
            }
            health -= damage;

            Console.WriteLine($"{this.type} took {damage} damage, {this.type} health = {health}");

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
            Console.WriteLine($"{this.type} was destroyed");
            Manager.pieces.Remove(this);
        }
        public void Display()
        {
            //set centre = this
            Game.Tile A = Game.Tile.Hexagons[position];
            shape.centre = 8 * new Vector3((A.qr.Item1 + A.qr.Item2 / 2f) * 2, -1, (float)Math.Sqrt(3) * (A.qr.Item2)) + new Vector3(0, 3 * A.height + 70, 0);
            return;
        }
        public void Move((int, int) newPos)
        {
            Manager.Move(this, newPos);
        }
        public List<(int, int)> GetMoves()
        {
            return Manager.getMoves(this);
        }

        public List<(int, int)> GetAttackable()
        {
            return Manager.GetAttackable(this);
        }
        public Fighter((int, int) position, int team = 0, int health = 40, int damage = 8)
        {
            this.shape = new Game.Shape("cube.obj", team == 0 ? Color.DarkBlue : Color.Red);
            this.shape.scale *= 7;
            this.shape.scale *= new Vector3(0.8f, 0.3f, 1.5f);
            this.position = position;
            this.team = team;
            this.health = health;
            this.damage = damage;
        }
    }

    class Bomber : Ientity
    {
        public Unit SpecialType { get; set; }
        public Game.Shape shape { get; set; }
        public (int, int) position { get; set; }
        public bool canAttack { get; set; } = true;
        public Unit type { get; set; } = Unit.Bomber;
        public Domain domain { get; set; } = Domain.Air;
        public armourType armour { get; set; } = armourType.Light;
        public damageType gunType { get; set; } = damageType.Bomb;
        public int moveSpeed { get; set; } = 3;
        public int range { get; set; } = 1;
        public int team { get; set; }

        public int health { get; set; } = 0;
        public int damage { get; set; }

        public void attack((int, int) position)
        {
            for (int i = 0; i < Manager.pieces.Count(); i++)
            {
                if (position == Manager.pieces[i].position && Manager.pieces[i].team != this.team)
                {
                    Damage(Manager.pieces[i]);
                }
            }
        }
        public void Damage(Ientity target)
        {
            target.TakeDamage(damage, this.gunType);
        }
        public void TakeDamage(int damage, damageType Damage)
        {
            if (Damage == damageType.Missile)
            {
                damage = (int)(damage * 2);
            }
            if (Damage == damageType.Gun)
            {
                damage = (int)(damage * 3/2);
            }
            health -= damage;

            Console.WriteLine($"{this.type} took {damage} damage, {this.type} health = {health}");

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
            Console.WriteLine($"{this.type} was destroyed");
            Manager.pieces.Remove(this);
        }
        public void Display()
        {
            //set centre = this
            Game.Tile A = Game.Tile.Hexagons[position];
            shape.centre = 8 * new Vector3((A.qr.Item1 + A.qr.Item2 / 2f) * 2, -1, (float)Math.Sqrt(3) * (A.qr.Item2)) + new Vector3(0, 3 * A.height + 80, 0);
            return;
        }
        public void Move((int, int) newPos)
        {
            Manager.Move(this, newPos);
        }
        public List<(int, int)> GetMoves()
        {
            return Manager.getMoves(this);
        }

        public List<(int, int)> GetAttackable()
        {
            return Manager.GetAttackable(this);
        }
        public Bomber((int, int) position, int team = 0, int health = 50, int damage = 20)
        {
            this.shape = new Game.Shape("cube.obj", team == 0 ? Color.DarkBlue : Color.Red);
            this.shape.scale *= 7;
            this.shape.scale *= new Vector3(1f, 0.4f, 1.5f);
            this.position = position;
            this.team = team;
            this.health = health;
            this.damage = damage;
        }
    }
    class Missile_Boat : Ientity
    {
        public Unit SpecialType { get; set; }
        public Game.Shape shape { get; set; }
        public (int, int) position { get; set; }
        public bool canAttack { get; set; } = true;
        public Unit type { get; set; } = Unit.Missile_Boat;
        public Domain domain { get; set; } = Domain.Water;
        public armourType armour { get; set; } = armourType.Light;
        public damageType gunType { get; set; } = damageType.Missile;
        public int moveSpeed { get; set; } = 4;
        public int range { get; set; } = 4;
        public int team { get; set; }

        public int health { get; set; } = 0;
        public int damage { get; set; }

        public void attack((int, int) position)
        {
            for (int i = 0; i < Manager.pieces.Count(); i++)
            {
                if (position == Manager.pieces[i].position && Manager.pieces[i].team != this.team)
                {
                    Damage(Manager.pieces[i]);
                }
            }
        }
        public void Damage(Ientity target)
        {
            target.TakeDamage(damage, this.gunType);
        }
        public void TakeDamage(int damage, damageType Damage)
        {
            if (Damage == damageType.Shell)
            {
                damage = (int)(damage * 2);
            }
            health -= damage;

            Console.WriteLine($"{this.type} took {damage} damage, {this.type} health = {health}");

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
            Console.WriteLine($"{this.type} was destroyed");
            Manager.pieces.Remove(this);
        }
        public void Display()
        {
            //set centre = this
            Game.Tile A = Game.Tile.Hexagons[position];
            shape.centre = 8 * new Vector3((A.qr.Item1 + A.qr.Item2 / 2f) * 2, -1, (float)Math.Sqrt(3) * (A.qr.Item2)) + new Vector3(0, 3 * A.height + 20, 0);
            return;
        }
        public void Move((int, int) newPos)
        {
            Manager.Move(this, newPos);
        }
        public List<(int, int)> GetMoves()
        {
            return Manager.getMoves(this);
        }

        public List<(int, int)> GetAttackable()
        {
            return Manager.GetAttackable(this);
        }
        public Missile_Boat((int, int) position, int team = 0, int health = 50, int damage = 10)
        {
            this.shape = new Game.Shape("cube.obj", team == 0 ? Color.DarkBlue : Color.Red);
            this.shape.scale *= 7;
            this.shape.scale *= new Vector3(1f, 0.4f, 1.5f);
            this.position = position;
            this.team = team;
            this.health = health;
            this.damage = damage;
        }
    }

    class AircraftCarrier : Ientity
    {
        public Unit SpecialType { get; set; } = Unit.Fighter;
        public int PlaneStock = 2;
       
        public Game.Shape shape { get; set; }
        public (int, int) position { get; set; }
        public bool canAttack { get; set; } = true;
        public Unit type { get; set; } = Unit.Aircraft_Carrier;
        public Domain domain { get; set; } = Domain.Water;
        public armourType armour { get; set; } = armourType.Heavy;
        public damageType gunType { get; set; } = damageType.Gun;
        public int moveSpeed { get; set; } = 1;
        public int range { get; set; } = 1;
        public int team { get; set; }

        public int health { get; set; } = 0;
        public int damage { get; set; }
        public void attack((int, int) position)
        {
            if (SpecialType == Unit.Bomber)
            {
                Manager.pieces.Add(Manager.storedPieces[this.team].Where(x => x.type == Unit.Bomber).First());
                Manager.storedPieces[team].Remove(Manager.storedPieces[this.team].Where(x => x.type == Unit.Bomber).First());
                Manager.pieces.Last().position = position;

                SpecialType = Unit.Fighter;
            }
            else
            {
                Manager.pieces.Add(Manager.storedPieces[this.team].Where(x => x.type == Unit.Fighter).First());
                Manager.storedPieces[team].Remove(Manager.storedPieces[this.team].Where(x => x.type == Unit.Fighter).First());
                Manager.pieces.Last().position = position;

                SpecialType = Unit.Bomber;
            }
            PlaneStock--;
        }
        public void Damage(Ientity target)
        {
            target.TakeDamage(damage, this.gunType);
        }
        public void TakeDamage(int damage, damageType Damage)
        {
            if (Damage == damageType.Shell)
            {
                damage = (int)(damage * 2);
            }
            health -= damage;

            Console.WriteLine($"{this.type} took {damage} damage, {this.type} health = {health}");

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
            Console.WriteLine($"{this.type} was destroyed");
            Manager.pieces.Remove(this);
        }
        public void Display()
        {
            //set centre = this
            Game.Tile A = Game.Tile.Hexagons[position];
            shape.centre = 8 * new Vector3((A.qr.Item1 + A.qr.Item2 / 2f) * 2, -1, (float)Math.Sqrt(3) * (A.qr.Item2)) + new Vector3(0, 3 * A.height + 20, 0);
            return;
        }
        public void Move((int, int) newPos)
        {
            Manager.Move(this, newPos);
        }
        public List<(int, int)> GetMoves()
        {
            return Manager.getMoves(this);
        }

        public List<(int, int)> GetAttackable()
        {
            if(this.PlaneStock == 0)
            {
                return new List<(int,int)>();
            }

            List<(int, int)> targettable = Hex.RangeN(position, range);

            HashSet<(int, int)> obstructions = new HashSet<(int, int)>();
            for (int i = 0; i < Manager.pieces.Count(); i++)
            {
                if (Manager.pieces[i] != this && Manager.pieces[i].domain == Domain.Air)
                {
                    obstructions.Add(Manager.pieces[i].position);
                }
            }


            for (int i = 0; i < targettable.Count; i++)
            {
                //see if targetable
                if (obstructions.Contains(targettable[i]))
                {
                    targettable.Remove(targettable[i]);
                    i--;
                }

            }
            return targettable;

        }
        public AircraftCarrier((int, int) position, int team = 0, int health = 100, int damage = 2)
        {
            this.shape = new Game.Shape("cube.obj", team == 0 ? Color.DarkBlue : Color.Red);
            this.shape.scale *= 7;
            this.shape.scale *= new Vector3(1f, 0.4f, 1.5f);
            this.position = position;
            this.team = team;
            this.health = health;
            this.damage = damage;
        }
    }


    class Manager
    {
        static int teamCount = 2;
        public static List<Ientity> pieces = new List<Ientity>();
        public static List<List<Ientity>> storedPieces = new List<List<Ientity>>();


        public static void setup()
        {
            //start stuff

            //pieces.Add(new Tank((0,0),0));
            //pieces.Add(new Missile_Boat((0,0),0));

            Random locationSelector = new Random();
            pieces.Add(new AircraftCarrier(Game.water[locationSelector.Next(0,Game.water.Count())].qr,1));
            pieces.Add(new AircraftCarrier(Game.water[locationSelector.Next(0, Game.water.Count())].qr, 0));


            for (int i = 0; i < teamCount;i++) 
            { 
                storedPieces.Add(new List<Ientity>() { new Fighter((0, 0),i),new Bomber((0,0),i)});
            }
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
                Console.WriteLine("----------");
                Console.WriteLine($"{(list[i].team == 1 ? "Red" : "Blue")} {list[i].type}");
                Console.WriteLine("health = " + list[i].health);

                //
                //get moves
                //

                List<(int, int)> moves = list[i].GetMoves();

                Game.DisplayMoves = moves.ToList();

                Game.turnStatus = "move";

                Console.WriteLine("select move");
                Thread.Sleep(100);
                while (true)
                {
                    if(moves.Contains(Game.curHex) && Game.currentKey.IsKeyDown(Key.Enter))
                    {
                        list[i].Move(Game.curHex);
                        break;
                    }
                }


                ATTACK(list[i]);
            }

            Thread.Sleep(100);
            Game.DoNextTurn(1 - currTeam);
            return;
        }
        static void ATTACK(Ientity CurrentPiece)
        {
            //
            //attack
            //

            List<(int, int)> attacks = CurrentPiece.GetAttackable();

            Game.DisplayMoves = attacks.ToList();

            Game.turnStatus = "attack";
            Thread.Sleep(100);
            if (CurrentPiece.type != Unit.Aircraft_Carrier)
            {
                if (attacks.Count() != 0)
                {
                    Console.WriteLine("select target");
                }
                while (true && attacks.Count() != 0)
                {

                    bool breaker = false;
                    if (attacks.Contains(Game.curHex))
                    {
                        breaker = false;
                        Console.WriteLine();
                        Console.WriteLine($"attack {pieces.Where(x => x.position == Game.curHex).FirstOrDefault().type}?");
                        Console.WriteLine("press enter to attack");

                        do
                        {
                            if (Game.currentKey.IsKeyDown(Key.Enter))
                            {
                                CurrentPiece.attack(Game.curHex);
                                breaker = true;
                            }
                            else if (!attacks.Contains(Game.curHex))
                            {
                                break;
                            }
                        } while (!Game.currentKey.IsKeyDown(Key.Enter));

                        if (breaker) break;

                    }

                    if (Game.currentKey.IsKeyDown(Key.BackSpace))
                    {
                        break;
                    }
                }
            }
            else
            {
                if (attacks.Count() != 0)
                {
                    Console.WriteLine("select spawn Location");
                }
                while (true && attacks.Count() != 0)
                {

                    bool breaker = false;
                    if (attacks.Contains(Game.curHex))
                    {
                        breaker = false;
                        Console.WriteLine();
                        Console.WriteLine($"spawn {CurrentPiece.SpecialType}?");
                        Console.WriteLine("press enter to spawn");

                        do
                        {
                            if (Game.currentKey.IsKeyDown(Key.Enter))
                            {
                                CurrentPiece.attack(Game.curHex);
                                breaker = true;
                            }
                            else if (!attacks.Contains(Game.curHex))
                            {
                                break;
                            }
                        } while (!Game.currentKey.IsKeyDown(Key.Enter));

                        if (breaker) break;

                    }

                    if (Game.currentKey.IsKeyDown(Key.BackSpace))
                    {
                        break;
                    }
                }
            }

        }



        public static List<(int, int)> getMoves(Ientity A)
        {
            try
            {
                //return Hex.RangeN(position, moveSpeed).Where(x => Game.Tile.Hexagons.Where(y => y.type == Game.pieces.Land).ToList().Contains()).ToList();
                HashSet<(int, int)> obstructions = new HashSet<(int, int)>();
                for (int i = 0; i < Manager.pieces.Count(); i++)
                {
                    if (Manager.pieces[i] != A && Manager.pieces[i].domain == A.domain)
                    {
                        obstructions.Add(Manager.pieces[i].position);
                    }
                }
                List<(int, int)> MOVES = Hex.RangeN(A.position, A.moveSpeed);
                List<(int, int)> validMoves = new List<(int, int)>();
                for (int i = 0; i < MOVES.Count(); i++)
                {
                    try
                    {
                        Game.Tile QR = Game.Tile.Hexagons[MOVES[i]];
                        if(A.domain == Domain.Land)
                        {
                            if ((QR.type == Game.pieces.Land || QR.type == Game.pieces.Shore) && !obstructions.Contains(QR.qr))
                            {
                                validMoves.Add(QR.qr);
                            }
                        }
                        else if (A.domain == Domain.Water)
                        {
                            if ((QR.type == Game.pieces.Water) && !obstructions.Contains(QR.qr))
                            {
                                validMoves.Add(QR.qr);
                            }
                        }
                        else
                        {
                            if (!obstructions.Contains(QR.qr))
                            {
                                validMoves.Add(QR.qr);
                            }
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
        public static void Move(Ientity A, (int, int) newPos)
        {
            (int, int) displacement = Hex.subtract(newPos, A.position);
            float angle = (float)Math.Atan2((displacement.Item1 + displacement.Item2), ((float)Math.Sqrt(3) * displacement.Item2));
            A.position = newPos;
            A.shape.angle = new Vector3(0, angle, 0);
        }

        public static List<(int, int)> GetAttackable(Ientity A)
        {
            List<(int, int)> targettable = Hex.RangeN(A.position, A.range);

            HashSet<(int, int)> targets = new HashSet<(int, int)>();
            for (int i = 0; i < Manager.pieces.Count(); i++)
            {
                if (Manager.pieces[i].team != A.team)
                {
                    targets.Add(Manager.pieces[i].position);
                }
            }

            for (int i = 0; i < targettable.Count; i++)
            {
                //see if targetable
                if (!targets.Contains(targettable[i]))
                {
                    targettable.Remove(targettable[i]);
                    i--;
                }

            }
            return targettable;

        }
    }

    internal class entity
    {

    }
}
