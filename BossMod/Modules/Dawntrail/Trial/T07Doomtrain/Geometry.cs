namespace BossMod.Dawntrail.Trial.T07Doomtrain;

/**
 *  This class creates persistent railcar objects called 'car'.  We can then reference what car
 *  we are on during various phases and boss abilities to determine how they interact with each car.
 *
 *  Additionally, the arenas for each car have various platforms and obstacles.  The railcars are (mostly) just
 *  rectangles with line outlines to give the illusion of topology.  The platforms use tempObstacles
 *  depending on pc height to allow pathfinding to recognize they exist.
 *  This is why the aoes do not clip properly when on a platform or on the lower deck.  The slight visual
 *  trade off was worthwhile compared to figuring out a polygon that could handle all of the aoe clipping.
 */
class CarGeometry : BossComponent
{
    public int Car
    {
        get => field;
        set
        {
            field = value;
            switch (value)
            {
                case 2:
                    Car2();
                    break;
                case 3:
                    Car3();
                    break;
                case 4:
                    Car4();
                    break;
                case 5:
                    Car5();
                    break;
            }
        }
    } = 1;

    public const float CarHeight = 15f;
    public List<Shape> Platforms = [];

    // Define the shapes of lower and upper decks so we can define map outlines and custom aoeshapes for each car.
    // Groundshape allows us to define the shape of a rail car without the platforms.
    public AOEShape GroundShape = new AOEShapeRect(CarHeight, 10f, CarHeight);
    // Airshape allows us to define the shape of the upper deck platforms.
    public AOEShape AirShape = new AOEShapeRect(CarHeight, 10f, CarHeight);

    public CarGeometry(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    // Draw the platform outlines here for the cars that have platforms
    // AirShape is defined in the respective car method.
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        switch (Car)
        {
            case 3:
                AirShape.Outline(Arena, new WPos(100f, 200f), default, Colors.Border);
                break;
            case 5:
                AirShape.Outline(Arena, new WPos(100f, 300f), default, Colors.Border);
                break;
        }
    }

    // Tell the AI how to navigate the teleporters and other pathfinding special cases.
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        switch (Car)
        {
            case 3:
                var carThreeLeftEntrance = new WPos(96.1f, 204.9f);
                var carThreeLeftExit = new WPos(92.4f, 204.9f);
                var carThreeRightEntrance = new WPos(104.1f, 204.9f);
                var carThreeRightExit = new WPos(107.6f, 204.9f);

                hints.Teleporters.Add(new(carThreeLeftEntrance, carThreeLeftExit, 1f, false));
                hints.Teleporters.Add(new(carThreeRightEntrance, carThreeRightExit, 1f, false));
                //Set temporary obstacles to pathfinding if you are on ground floor.
                if (actor.PosRot.Y < 4)
                {
                    // Space between the blocks is left in place so teleporter exit can be navigated.
                    // Intentionally extra wide to prevent pc from squeezing between teleport and platform and having a seizure.
                    //Right side
                    hints.TemporaryObstacles.Add(new SDRect(new(92.5f, 195f), new(92.5f, 204f), 3f));
                    hints.TemporaryObstacles.Add(new SDRect(new(92.5f, 206f), new(92.5f, 215f), 3f));
                    //Left side
                    hints.TemporaryObstacles.Add(new SDRect(new(107.5f, 195f), new(107.5f, 204f), 3f));
                    hints.TemporaryObstacles.Add(new SDRect(new(107.5f, 206f), new(107.5f, 215f), 3f));
                }
                else
                {
                    // Obstacles placed over teleport pads to prevent pathing back down onto teleport from platform.
                    // while on upper deck only.
                    hints.TemporaryObstacles.Add(new SDCircle(carThreeLeftEntrance, 1.5f));
                    hints.TemporaryObstacles.Add(new SDCircle(carThreeRightEntrance, 1.5f));
                }
                break;
            case 5:
                var carFiveLeftEntrance = new WPos(96.1f, 310f);
                var carFiveLeftExit = new WPos(92.4f, 310f);
                var carFiveRightEntrance = new WPos(104.1f, 300f);
                var carFiveRightExit = new WPos(107.6f, 300f);

                hints.Teleporters.Add(new(carFiveLeftEntrance, carFiveLeftExit, 1f, false));
                hints.Teleporters.Add(new(carFiveRightEntrance, carFiveRightExit, 1f, false));
                //Set temporary obstacles to pathfinding if you are on ground floor.
                if (actor.PosRot.Y < 4f)
                {
                    // Space between the blocks is left in place so teleporter exit can be navigated.
                    // Intentionally extra wide to prevent pc from squeezing between teleport and platform and having a seizure.
                    // Left side
                    hints.TemporaryObstacles.Add(new SDRect(new(92.5f, 305f), new(92.5f, 309f), 3f));
                    hints.TemporaryObstacles.Add(new SDRect(new(92.5f, 311f), new(92.5f, 315f), 3f));
                    // Right side
                    hints.TemporaryObstacles.Add(new SDRect(new(107.5f, 295f), new(107.5f, 299f), 3f));
                    hints.TemporaryObstacles.Add(new SDRect(new(107.5f, 301f), new(107.5f, 305f), 3));
                }
                else
                {
                    // Obstacles placed over teleport pads to prevent pathing back down onto teleport from platform.
                    // while on upper deck only.
                    hints.TemporaryObstacles.Add(new SDCircle(carFiveLeftEntrance, 1.5f));
                    hints.TemporaryObstacles.Add(new SDCircle(carFiveRightEntrance, 1.5f));
                }
                break;
        }
    }

    // Draws the outlines for the platforms. Pathfinding around platforms added in AIHints.
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var color = Colors.SafeFromAOE;

        switch (Car)
        {
            case 3:
                var carThreeLeftEntrance = new WPos(96.1f, 204.9f);
                var carThreeRightEntrance = new WPos(104.1f, 204.9f);

                Arena.ZoneCircle(carThreeRightEntrance, 1f, color);
                Arena.ZoneCircle(carThreeLeftEntrance, 1f, color);
                break;

            case 5:
                var carFiveLeftEntrance = new WPos(96.1f, 310f);
                var carFiveRightEntrance = new WPos(104.1f, 300f);

                Arena.ZoneCircle(carFiveRightEntrance, 1f, color);
                Arena.ZoneCircle(carFiveLeftEntrance, 1f, color);
                break;
        }
    }

    void Car2()
    {
        var carTwoCenter = new WPos(100f, 150f);
        Shape[] carOutline = [new Rectangle(carTwoCenter, 10f, 15f)];
        Shape[] carTwoCrates = [new Square(new(102.5f, 147.5f), 2.5f), new Square(new(97.5f, 157.5f), 2.5f)];

        ArenaBoundsCustom carTwo = new(carOutline, carTwoCrates);

        Arena.Center = new(100f, 150f);
        Arena.Bounds = carTwo;
    }

    void Car3()
    {
        // Car3 has a platform on the right and left side.  Each with a portal
        // to jump up
        var carThreeCenter = new WPos(100f, 200f);
        Shape[] carOutline = [new Rectangle(carThreeCenter, 10f, 15f)];
        //blocking shapes on ground level for platforms
        Platforms = [new Rectangle(new(92.5f, 205f), 2.4f, 10f), new Rectangle(new(107.5f, 205f), 2.4f, 10f)];

        // AOE shape for an AOE that would cover the car three lower deck.
        // used for head on emission (lower)
        GroundShape = new AOEShapeCustom(carThreeCenter, carOutline, Platforms);

        // AOE shape that covers both right and left platforms
        // Used for head on emission (upper)
        AirShape = new AOEShapeCustom(carThreeCenter, Platforms);

        //This arena bounds draws the lower deck and upper deck, but treats the platforms as unpassable holes.
        ArenaBoundsCustom carThreeOutline = new(carOutline, null);

        Arena.Center = carThreeCenter;
        Arena.Bounds = carThreeOutline;
    }

    void Car4()
    {
        Arena.Bounds = new ArenaBoundsRect(10f, 14.6f);
        Arena.Center = new(100f, 250f);
    }

    void Car5()
    {
        //car 5 has platforms on right and left placed slightly off from each other
        var carFiveCenter = new WPos(100f, 300f);
        Shape[] carOutline = [new Rectangle(carFiveCenter, 10f, 15f)];

        // upper deck should have rectangles in the same size as platforms.
        // with wall boundary on outer rectangle, no boundary inside outer rectangle bounds.
        // ai hints are used to set pathfinding above.
        Platforms = [new Rectangle(new(92.4f, 310f), 2.3f, 5f), new Rectangle(new(107.6f, 300f), 2.3f, 5f)];

        // AOE shape for an AOE that would cover car five lower deck.
        // used for head on emission (lower)
        GroundShape = new AOEShapeCustom(carFiveCenter, carOutline, Platforms);
        // AOE shape that covers both right and left platforms
        // Used for head on emission (upper)
        AirShape = new AOEShapeCustom(carFiveCenter, Platforms);

        ArenaBoundsCustom carFiveOutline = new(carOutline);
        Arena.Center = carFiveCenter;
        Arena.Bounds = carFiveOutline;
    }
}
