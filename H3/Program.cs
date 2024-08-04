using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Bungie;
using Bungie.Tags;

namespace H3 {
    internal class Program {
        public static void Main(string[] args) {
            const string h3ek = @"E:\Steam\steamapps\common\H3EK";

            ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ek);

            int choice = -1;

            switch (choice) {
                case 0: {
                    var src = TagPath.FromPathAndType(@"levels\solo\020_base\020_base", "scnr*");
                    var dst = TagPath.FromPathAndType(@"mod\020_base_part2_jv_less", "scnr*");
                    BlamTask1(src, dst, 100, 0);
                    break;
                }

                case 1: {
                    var scnr_tag_path = TagPath.FromPathAndType(@"mod\020_base_part2_jv_less", "scnr*");
                    BlamTask2(scnr_tag_path);
                    break;
                }

                default:
                    break;
            }
        }

        /// <summary>
        /// Read mv_scenery_palette from a scenario and add multiplayer_object block if it doesn't exist
        /// </summary>
        private static void BlamTask2(TagPath scnr_tag_path) {
            int count = 0;
            
            var tag = new TagFile(scnr_tag_path);

            var block = (TagFieldBlock)tag.Fields[(int)ScenarioField.mv_scenery_palette];
            
            Console.WriteLine($"Total elements: {block.Elements.Count}");
            
            foreach (var element in block) {
                var name = (TagFieldReference)element.Fields[0];

                Console.WriteLine($"Writing to {name.Path}");

                var target_tag = new TagFile(name.Path);
                var OBJECT = (TagFieldStruct)target_tag.Fields[0];
                var multiplayer_object = (TagFieldBlock)OBJECT.Elements[0].Fields[(int)ObjectField.multiplayer_object];

                if (multiplayer_object.Elements.Count == 0) {
                    multiplayer_object.AddElement();
                    target_tag.Save();
                    count++;
                }
            }
            
            Console.WriteLine("Done");
            Console.WriteLine($"Added {count} Blocks");
            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();
        }

        /// <summary>
        /// Copy scenery and crate palettes from one scenario to mv_scenery_palette in another scenario
        /// </summary>
        private static void BlamTask1(TagPath src, TagPath dst, long maximum_allowed, float price_per_instance) {
            var list = new List<TagPath>();

            var src_tag = new TagFile(src);
            var mod_tag = new TagFile(dst);

            list.AddRange(
                ((TagFieldBlock)src_tag.Fields[(int)ScenarioField.scenery_palette])
                .Select(scenery => ((TagFieldReference)scenery.Fields[0]).Path)
            );
            
            list.AddRange(
                ((TagFieldBlock)src_tag.Fields[(int)ScenarioField.crate_palette])
                .Select(crate => ((TagFieldReference)crate.Fields[0]).Path)
            );
            
            var mv_scenert_palette = (TagFieldBlock)mod_tag.Fields[(int)ScenarioField.mv_scenery_palette];
            
            foreach (var path in list) {
                var ele = new MVSceneryPalette(mv_scenert_palette.AddElement());
                
                ele.name.Path = path;
                ele.display_name.Data = path.ShortName;
                ele.maximum_allowed.Data = maximum_allowed;
                ele.price_per_instance.Data = price_per_instance;
            }
            
            mod_tag.Save();
        }
    }
}