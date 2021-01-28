using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Nami.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "nami");

            migrationBuilder.CreateTable(
                name: "blocked_channels",
                schema: "nami",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    reason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blocked_channels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "blocked_guilds",
                schema: "nami",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    reason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blocked_guilds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "blocked_users",
                schema: "nami",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    reason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blocked_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bot_statuses",
                schema: "nami",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    activity_type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bot_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "chicken_upgrades",
                schema: "nami",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    cost = table.Column<long>(type: "bigint", nullable: false),
                    stat = table.Column<int>(type: "integer", nullable: false),
                    mod = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chicken_upgrades", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "game_stats",
                schema: "nami",
                columns: table => new
                {
                    uid = table.Column<long>(type: "bigint", nullable: false),
                    duel_won = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    duel_lost = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    hangman_won = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    quiz_won = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ar_won = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    nr_won = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    tr_won = table.Column<int>(type: "integer", nullable: false),
                    rr_won = table.Column<int>(type: "integer", nullable: false),
                    ttt_won = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ttt_lost = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    c4_won = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    c4_lost = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    caro_won = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    caro_lost = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    othello_won = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    othello_lost = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_stats", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "guild_cfg",
                schema: "nami",
                columns: table => new
                {
                    antiflood_action = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)4),
                    antiflood_cooldown = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)10),
                    antiflood_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    antiflood_sensitivity = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)5),
                    antiinstantleave_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    antiinstantleave_cooldown = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)3),
                    antispam_action = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    antispam_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    antispam_sensitivity = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)5),
                    backup = table.Column<bool>(type: "boolean", nullable: false),
                    currency = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    leave_cid = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    leave_msg = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    linkfilter_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    linkfilter_booters = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    linkfilter_disturbing = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    linkfilter_invites = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    linkfilter_loggers = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    linkfilter_shorteners = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    locale = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    log_cid = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    mute_rid = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    prefix = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    ratelimit_action = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)1),
                    ratelimit_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ratelimit_sensitivity = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)5),
                    suggestions_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    streamer_badge = table.Column<string>(type: "character verying(128)", nullable: true, defaultValue: null),
                    silent_response_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    starboard_cid = table.Column<long>(type: "bigint", nullable: false),
                    starboard_emoji = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    starboard_sens = table.Column<int>(type: "integer", nullable: false),
                    timezone_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    welcome_cid = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    welcome_msg = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_cfg", x => x.gid);
                });

            migrationBuilder.CreateTable(
                name: "privileged_users",
                schema: "nami",
                columns: table => new
                {
                    uid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_privileged_users", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "reminders",
                schema: "nami",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cid = table.Column<long>(type: "bigint", nullable: true),
                    message = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    is_repeating = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    repeat_interval = table.Column<TimeSpan>(type: "interval", nullable: true, defaultValue: new TimeSpan(0, 0, 0, 0, -1)),
                    uid = table.Column<long>(type: "bigint", nullable: false),
                    execution_time = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reminders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rss_feeds",
                schema: "nami",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    last_post_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rss_feeds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "xp_count",
                schema: "nami",
                columns: table => new
                {
                    uid = table.Column<long>(type: "bigint", nullable: false),
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    xp = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_xp_count", x => new { x.gid, x.uid });
                });

            migrationBuilder.CreateTable(
                name: "auto_roles",
                schema: "nami",
                columns: table => new
                {
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    rid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auto_roles", x => new { x.gid, x.rid });
                    table.ForeignKey(
                        name: "FK_auto_roles_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bank_accounts",
                schema: "nami",
                columns: table => new
                {
                    uid = table.Column<long>(type: "bigint", nullable: false),
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    balance = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_accounts", x => new { x.gid, x.uid });
                    table.ForeignKey(
                        name: "FK_bank_accounts_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "birthdays",
                schema: "nami",
                columns: table => new
                {
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    uid = table.Column<long>(type: "bigint", nullable: false),
                    cid = table.Column<long>(type: "bigint", nullable: false),
                    date = table.Column<DateTime>(type: "date", nullable: false),
                    last_update_year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_birthdays", x => new { x.gid, x.cid, x.uid });
                    table.ForeignKey(
                        name: "FK_birthdays_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chickens",
                schema: "nami",
                columns: table => new
                {
                    uid = table.Column<long>(type: "bigint", nullable: false),
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    str = table.Column<int>(type: "integer", nullable: false),
                    vit = table.Column<int>(type: "integer", nullable: false),
                    max_vit = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chickens", x => new { x.gid, x.uid });
                    table.ForeignKey(
                        name: "FK_chickens_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cmd_rules",
                schema: "nami",
                columns: table => new
                {
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    cid = table.Column<long>(type: "bigint", nullable: false),
                    command = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    allow = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cmd_rules", x => new { x.gid, x.cid, x.command });
                    table.ForeignKey(
                        name: "FK_cmd_rules_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exempt_antispam",
                schema: "nami",
                columns: table => new
                {
                    xid = table.Column<long>(type: "bigint", nullable: false),
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exempt_antispam", x => new { x.xid, x.gid, x.type });
                    table.ForeignKey(
                        name: "FK_exempt_antispam_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exempt_backup",
                schema: "nami",
                columns: table => new
                {
                    cid = table.Column<long>(type: "bigint", nullable: false),
                    gid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exempt_backup", x => new { x.gid, x.cid });
                    table.ForeignKey(
                        name: "FK_exempt_backup_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exempt_logging",
                schema: "nami",
                columns: table => new
                {
                    xid = table.Column<long>(type: "bigint", nullable: false),
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exempt_logging", x => new { x.xid, x.gid, x.type });
                    table.ForeignKey(
                        name: "FK_exempt_logging_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exempt_ratelimit",
                schema: "nami",
                columns: table => new
                {
                    xid = table.Column<long>(type: "bigint", nullable: false),
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exempt_ratelimit", x => new { x.xid, x.gid, x.type });
                    table.ForeignKey(
                        name: "FK_exempt_ratelimit_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "filters",
                schema: "nami",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    trigger = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_filters", x => x.id);
                    table.ForeignKey(
                        name: "FK_filters_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });


            migrationBuilder.CreateTable(
               name: "streams",
               schema: "nami",
               columns: table => new 
               {
                   id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                   gid = table.Column<long>(type: "bigint", nullable: false),
                   member_id = table.Column<long>(type: "bigint", maxLength: 128, nullable: false),
                   joined = table.Column<int>(type: "integer", maxLength: 128, nullable: false),
               },
               constraints: table => {
                   table.PrimaryKey("PK_streams", x => x.id);
                   table.ForeignKey(
                       name: "FK_streams_guild_cfg_gid",
                       column: x => x.gid,
                       principalSchema: "nami",
                       principalTable: "guild_cfg",
                       principalColumn: "gid",
                       onDelete: ReferentialAction.Cascade);
               });










            migrationBuilder.CreateTable(
                name: "forbidden_names",
                schema: "nami",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    name_regex = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forbidden_names", x => x.id);
                    table.ForeignKey(
                        name: "FK_forbidden_names_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "guild_ranks",
                schema: "nami",
                columns: table => new
                {
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    rank = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_ranks", x => new { x.gid, x.rank });
                    table.ForeignKey(
                        name: "FK_guild_ranks_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "level_roles",
                schema: "nami",
                columns: table => new
                {
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    rank = table.Column<short>(type: "smallint", nullable: false),
                    rid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_level_roles", x => new { x.gid, x.rank });
                    table.ForeignKey(
                        name: "FK_level_roles_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "memes",
                schema: "nami",
                columns: table => new
                {
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    url = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memes", x => new { x.gid, x.name });
                    table.ForeignKey(
                        name: "FK_memes_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchasable_items",
                schema: "nami",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    price = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchasable_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_purchasable_items_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reaction_roles",
                schema: "nami",
                columns: table => new
                {
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    emoji = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    rid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reaction_roles", x => new { x.gid, x.emoji });
                    table.ForeignKey(
                        name: "FK_reaction_roles_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reactions_emoji",
                schema: "nami",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    reaction = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reactions_emoji", x => x.id);
                    table.ForeignKey(
                        name: "FK_reactions_emoji_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reactions_text",
                schema: "nami",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    reaction = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reactions_text", x => x.id);
                    table.ForeignKey(
                        name: "FK_reactions_text_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scheduled_tasks",
                schema: "nami",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    rid = table.Column<long>(type: "bigint", nullable: true),
                    type = table.Column<byte>(type: "smallint", nullable: false),
                    uid = table.Column<long>(type: "bigint", nullable: false),
                    execution_time = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduled_tasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_scheduled_tasks_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "self_roles",
                schema: "nami",
                columns: table => new
                {
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    rid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_self_roles", x => new { x.gid, x.rid });
                    table.ForeignKey(
                        name: "FK_self_roles_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "starboard",
                schema: "nami",
                columns: table => new
                {
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    cid = table.Column<long>(type: "bigint", nullable: false),
                    mid = table.Column<long>(type: "bigint", nullable: false),
                    smid = table.Column<long>(type: "bigint", nullable: false),
                    stars = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_starboard", x => new { x.gid, x.cid, x.mid });
                    table.ForeignKey(
                        name: "FK_starboard_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rss_subscriptions",
                schema: "nami",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    gid = table.Column<long>(type: "bigint", nullable: false),
                    cid = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rss_subscriptions", x => new { x.id, x.gid, x.cid });
                    table.ForeignKey(
                        name: "FK_rss_subscriptions_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rss_subscriptions_rss_feeds_id",
                        column: x => x.id,
                        principalSchema: "nami",
                        principalTable: "rss_feeds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chicken_bought_upgrades",
                schema: "nami",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    uid = table.Column<long>(type: "bigint", nullable: false),
                    gid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chicken_bought_upgrades", x => new { x.id, x.gid, x.uid });
                    table.ForeignKey(
                        name: "FK_chicken_bought_upgrades_chicken_upgrades_id",
                        column: x => x.id,
                        principalSchema: "nami",
                        principalTable: "chicken_upgrades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_chicken_bought_upgrades_chickens_gid_uid",
                        columns: x => new { x.gid, x.uid },
                        principalSchema: "nami",
                        principalTable: "chickens",
                        principalColumns: new[] { "gid", "uid" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_chicken_bought_upgrades_guild_cfg_gid",
                        column: x => x.gid,
                        principalSchema: "nami",
                        principalTable: "guild_cfg",
                        principalColumn: "gid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchased_items",
                schema: "nami",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    uid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchased_items", x => new { x.id, x.uid });
                    table.ForeignKey(
                        name: "FK_purchased_items_purchasable_items_id",
                        column: x => x.id,
                        principalSchema: "nami",
                        principalTable: "purchasable_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reactions_emoji_triggers",
                schema: "nami",
                columns: table => new
                {
                    trigger = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reactions_emoji_triggers", x => new { x.id, x.trigger });
                    table.ForeignKey(
                        name: "FK_reactions_emoji_triggers_reactions_emoji_id",
                        column: x => x.id,
                        principalSchema: "nami",
                        principalTable: "reactions_emoji",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reactions_text_triggers",
                schema: "nami",
                columns: table => new
                {
                    trigger = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reactions_text_triggers", x => new { x.id, x.trigger });
                    table.ForeignKey(
                        name: "FK_reactions_text_triggers_reactions_text_id",
                        column: x => x.id,
                        principalSchema: "nami",
                        principalTable: "reactions_text",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chicken_bought_upgrades_gid_uid",
                schema: "nami",
                table: "chicken_bought_upgrades",
                columns: new[] { "gid", "uid" });

            migrationBuilder.CreateIndex(
                name: "IX_exempt_antispam_gid",
                schema: "nami",
                table: "exempt_antispam",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_exempt_logging_gid",
                schema: "nami",
                table: "exempt_logging",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_exempt_ratelimit_gid",
                schema: "nami",
                table: "exempt_ratelimit",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_filters_gid",
                schema: "nami",
                table: "filters",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_forbidden_names_gid",
                schema: "nami",
                table: "forbidden_names",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_purchasable_items_gid",
                schema: "nami",
                table: "purchasable_items",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_reactions_emoji_gid",
                schema: "nami",
                table: "reactions_emoji",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_reactions_text_gid",
                schema: "nami",
                table: "reactions_text",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_rss_subscriptions_gid",
                schema: "nami",
                table: "rss_subscriptions",
                column: "gid");

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_tasks_gid",
                schema: "nami",
                table: "scheduled_tasks",
                column: "gid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auto_roles",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "bank_accounts",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "birthdays",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "blocked_channels",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "blocked_guilds",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "blocked_users",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "bot_statuses",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "chicken_bought_upgrades",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "cmd_rules",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "exempt_antispam",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "exempt_backup",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "exempt_logging",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "exempt_ratelimit",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "filters",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "forbidden_names",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "game_stats",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "guild_ranks",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "level_roles",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "memes",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "privileged_users",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "purchased_items",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "reaction_roles",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "reactions_emoji_triggers",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "reactions_text_triggers",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "reminders",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "rss_subscriptions",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "scheduled_tasks",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "self_roles",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "starboard",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "xp_count",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "chicken_upgrades",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "chickens",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "purchasable_items",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "reactions_emoji",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "reactions_text",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "rss_feeds",
                schema: "nami");

            migrationBuilder.DropTable(
                name: "guild_cfg",
                schema: "nami");
        }
    }
}
