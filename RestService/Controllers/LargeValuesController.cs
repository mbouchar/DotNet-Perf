using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RestService.Models;

namespace RestService.Controllers
{
    [Produces("application/json")]
    [Route("api/largevalues")]
    public class LargeValuesController : Controller
    {
        // GET: api/LargeValues
        [HttpGet]
        public IEnumerable<LargeDataStructure> Get()
        {
            var largeData = new LargeDataStructure()
            {
                Age = 1,
                FirstName = "Adolph Blaine Charles David Earl Frederick Gerald Hubert Irvim John Kenneth Loyd Martin Nero Oliver Paul Quincy Randolph Sherman Thomas Uncas Victor Willian Xerxes Yancy Zeus ",
                LastName = "Wolfeschlegelsteinhausenbergerdorffvoralternwarengewissenhaftschafers wesenchafewarenwholgepflegeundsorgfaltigkeitbeschutzenvonangereifen duchihrraubgiriigfeindewelchevorralternzwolftausendjahresvorandieer scheinenbanderersteerdeemmeshedrraumschiffgebrauchlichtalsseinu rsprungvonkraftgestartseinlangefahrthinzwischensternartigraumaufde rsuchenachdiesternwelshegehabtbewohnbarplanetenkreisedrehensichund wohinderneurassevanverstandigmenshlichkeittkonntevortpflanzenundsiche rfreunanlebenslamdlichfreudeundruhemitnichteinfurchtvorangreifenvon andererintlligentgeschopfsvonhinzwischensternartigraum",
                Country = "The United Kingdom of Great Britain and Northern Ireland",
                City = "Cambridgeshire and Isle of Ely",
                Description = "A descendant of one who prepared wool for manufacture on a stone, living in a house in the mountain village, who before ages was a conscientious shepherd whose sheep were well tended and diligently protected against attackers who by their rapacity were enemies who 12,000 years ago appeared from the stars to the humans by spaceships with light as an origin of power, started a long voyage within starlike space in search for the star which has habitable planets orbiting and on which the new race of reasonable humanity could thrive and enjoy lifelong happiness and tranquility without fear of attack from other intelligent creatures from within starlike space"
            };

            List<LargeDataStructure> data = new List<LargeDataStructure>();
            for (int i = 0; i < 500; i++)
                data.Add(largeData);

            return data;
        }
    }
}
