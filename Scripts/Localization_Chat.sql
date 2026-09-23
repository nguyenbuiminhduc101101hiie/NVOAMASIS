/*
  Chat nội bộ (Components/Chat) — chuỗi giao diện + mã lỗi ChatException.
  Safe to re-run.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

;WITH src AS (
    SELECT N'chat_title' AS ResourceKey, N'en-US' AS Culture, N'Chat' AS Value UNION ALL
    SELECT N'chat_title', N'vi-VN', N'Chat' UNION ALL
    SELECT N'chat_title', N'zh-CN', N'聊天' UNION ALL

    SELECT N'chat_new_conversation', N'en-US', N'New message' UNION ALL
    SELECT N'chat_new_conversation', N'vi-VN', N'Tin nhắn mới' UNION ALL
    SELECT N'chat_new_conversation', N'zh-CN', N'新消息' UNION ALL

    SELECT N'chat_search_conversation', N'en-US', N'Search conversations' UNION ALL
    SELECT N'chat_search_conversation', N'vi-VN', N'Tìm hội thoại' UNION ALL
    SELECT N'chat_search_conversation', N'zh-CN', N'搜索会话' UNION ALL

    SELECT N'chat_search_user', N'en-US', N'Search users' UNION ALL
    SELECT N'chat_search_user', N'vi-VN', N'Tìm người dùng' UNION ALL
    SELECT N'chat_search_user', N'zh-CN', N'搜索用户' UNION ALL

    SELECT N'chat_search_messages', N'en-US', N'Search in conversation' UNION ALL
    SELECT N'chat_search_messages', N'vi-VN', N'Tìm trong hội thoại' UNION ALL
    SELECT N'chat_search_messages', N'zh-CN', N'在会话中搜索' UNION ALL

    SELECT N'chat_no_conversations', N'en-US', N'No conversations yet' UNION ALL
    SELECT N'chat_no_conversations', N'vi-VN', N'Chưa có hội thoại nào' UNION ALL
    SELECT N'chat_no_conversations', N'zh-CN', N'暂无会话' UNION ALL

    SELECT N'chat_no_match', N'en-US', N'No matching results' UNION ALL
    SELECT N'chat_no_match', N'vi-VN', N'Không tìm thấy kết quả phù hợp' UNION ALL
    SELECT N'chat_no_match', N'zh-CN', N'没有匹配的结果' UNION ALL

    SELECT N'chat_no_messages', N'en-US', N'No messages yet. Say hello!' UNION ALL
    SELECT N'chat_no_messages', N'vi-VN', N'Chưa có tin nhắn. Hãy gửi lời chào!' UNION ALL
    SELECT N'chat_no_messages', N'zh-CN', N'暂无消息，打个招呼吧！' UNION ALL

    SELECT N'chat_select_conversation', N'en-US', N'Select a conversation or start a new one' UNION ALL
    SELECT N'chat_select_conversation', N'vi-VN', N'Chọn một hội thoại hoặc tạo tin nhắn mới' UNION ALL
    SELECT N'chat_select_conversation', N'zh-CN', N'选择一个会话或新建消息' UNION ALL

    SELECT N'chat_load_older', N'en-US', N'Load older messages' UNION ALL
    SELECT N'chat_load_older', N'vi-VN', N'Xem tin cũ hơn' UNION ALL
    SELECT N'chat_load_older', N'zh-CN', N'加载更早的消息' UNION ALL

    SELECT N'chat_input_placeholder', N'en-US', N'Type a message… (Enter to send, Shift+Enter for new line)' UNION ALL
    SELECT N'chat_input_placeholder', N'vi-VN', N'Nhập tin nhắn… (Enter để gửi, Shift+Enter xuống dòng)' UNION ALL
    SELECT N'chat_input_placeholder', N'zh-CN', N'输入消息…（Enter 发送，Shift+Enter 换行）' UNION ALL

    SELECT N'chat_send', N'en-US', N'Send' UNION ALL
    SELECT N'chat_send', N'vi-VN', N'Gửi' UNION ALL
    SELECT N'chat_send', N'zh-CN', N'发送' UNION ALL

    SELECT N'chat_attach', N'en-US', N'Attach file' UNION ALL
    SELECT N'chat_attach', N'vi-VN', N'Đính kèm file' UNION ALL
    SELECT N'chat_attach', N'zh-CN', N'附加文件' UNION ALL

    SELECT N'chat_is_typing', N'en-US', N'is typing…' UNION ALL
    SELECT N'chat_is_typing', N'vi-VN', N'đang nhập…' UNION ALL
    SELECT N'chat_is_typing', N'zh-CN', N'正在输入…' UNION ALL

    SELECT N'chat_many_typing', N'en-US', N'Several people are typing…' UNION ALL
    SELECT N'chat_many_typing', N'vi-VN', N'Nhiều người đang nhập…' UNION ALL
    SELECT N'chat_many_typing', N'zh-CN', N'多人正在输入…' UNION ALL

    SELECT N'chat_sent', N'en-US', N'Sent' UNION ALL
    SELECT N'chat_sent', N'vi-VN', N'Đã gửi' UNION ALL
    SELECT N'chat_sent', N'zh-CN', N'已发送' UNION ALL

    SELECT N'chat_seen', N'en-US', N'Seen' UNION ALL
    SELECT N'chat_seen', N'vi-VN', N'Đã xem' UNION ALL
    SELECT N'chat_seen', N'zh-CN', N'已读' UNION ALL

    SELECT N'chat_seen_by', N'en-US', N'Seen by' UNION ALL
    SELECT N'chat_seen_by', N'vi-VN', N'Đã xem bởi' UNION ALL
    SELECT N'chat_seen_by', N'zh-CN', N'已读人数' UNION ALL

    SELECT N'chat_today', N'en-US', N'Today' UNION ALL
    SELECT N'chat_today', N'vi-VN', N'Hôm nay' UNION ALL
    SELECT N'chat_today', N'zh-CN', N'今天' UNION ALL

    SELECT N'chat_yesterday', N'en-US', N'Yesterday' UNION ALL
    SELECT N'chat_yesterday', N'vi-VN', N'Hôm qua' UNION ALL
    SELECT N'chat_yesterday', N'zh-CN', N'昨天' UNION ALL

    SELECT N'chat_direct', N'en-US', N'Direct' UNION ALL
    SELECT N'chat_direct', N'vi-VN', N'Nhắn riêng' UNION ALL
    SELECT N'chat_direct', N'zh-CN', N'私聊' UNION ALL

    SELECT N'chat_group', N'en-US', N'Group' UNION ALL
    SELECT N'chat_group', N'vi-VN', N'Nhóm' UNION ALL
    SELECT N'chat_group', N'zh-CN', N'群组' UNION ALL

    SELECT N'chat_group_name', N'en-US', N'Group name' UNION ALL
    SELECT N'chat_group_name', N'vi-VN', N'Tên nhóm' UNION ALL
    SELECT N'chat_group_name', N'zh-CN', N'群组名称' UNION ALL

    SELECT N'chat_selected', N'en-US', N'Selected' UNION ALL
    SELECT N'chat_selected', N'vi-VN', N'Đã chọn' UNION ALL
    SELECT N'chat_selected', N'zh-CN', N'已选择' UNION ALL

    SELECT N'chat_start', N'en-US', N'Start' UNION ALL
    SELECT N'chat_start', N'vi-VN', N'Bắt đầu' UNION ALL
    SELECT N'chat_start', N'zh-CN', N'开始' UNION ALL

    SELECT N'chat_create_group', N'en-US', N'Create group' UNION ALL
    SELECT N'chat_create_group', N'vi-VN', N'Tạo nhóm' UNION ALL
    SELECT N'chat_create_group', N'zh-CN', N'创建群组' UNION ALL

    SELECT N'chat_cancel', N'en-US', N'Cancel' UNION ALL
    SELECT N'chat_cancel', N'vi-VN', N'Huỷ' UNION ALL
    SELECT N'chat_cancel', N'zh-CN', N'取消' UNION ALL

    SELECT N'chat_close', N'en-US', N'Close' UNION ALL
    SELECT N'chat_close', N'vi-VN', N'Đóng' UNION ALL
    SELECT N'chat_close', N'zh-CN', N'关闭' UNION ALL

    SELECT N'chat_members', N'en-US', N'members' UNION ALL
    SELECT N'chat_members', N'vi-VN', N'thành viên' UNION ALL
    SELECT N'chat_members', N'zh-CN', N'成员' UNION ALL

    SELECT N'chat_owner', N'en-US', N'Owner' UNION ALL
    SELECT N'chat_owner', N'vi-VN', N'Trưởng nhóm' UNION ALL
    SELECT N'chat_owner', N'zh-CN', N'群主' UNION ALL

    SELECT N'chat_rename', N'en-US', N'Rename' UNION ALL
    SELECT N'chat_rename', N'vi-VN', N'Đổi tên' UNION ALL
    SELECT N'chat_rename', N'zh-CN', N'重命名' UNION ALL

    SELECT N'chat_saved', N'en-US', N'Saved' UNION ALL
    SELECT N'chat_saved', N'vi-VN', N'Đã lưu' UNION ALL
    SELECT N'chat_saved', N'zh-CN', N'已保存' UNION ALL

    SELECT N'chat_add_members', N'en-US', N'Add members' UNION ALL
    SELECT N'chat_add_members', N'vi-VN', N'Thêm thành viên' UNION ALL
    SELECT N'chat_add_members', N'zh-CN', N'添加成员' UNION ALL

    SELECT N'chat_remove_member', N'en-US', N'Remove from group' UNION ALL
    SELECT N'chat_remove_member', N'vi-VN', N'Xoá khỏi nhóm' UNION ALL
    SELECT N'chat_remove_member', N'zh-CN', N'移出群组' UNION ALL

    SELECT N'chat_leave_group', N'en-US', N'Leave group' UNION ALL
    SELECT N'chat_leave_group', N'vi-VN', N'Rời nhóm' UNION ALL
    SELECT N'chat_leave_group', N'zh-CN', N'退出群组' UNION ALL

    SELECT N'chat_leave_confirm', N'en-US', N'Are you sure you want to leave this group?' UNION ALL
    SELECT N'chat_leave_confirm', N'vi-VN', N'Bạn chắc chắn muốn rời nhóm này?' UNION ALL
    SELECT N'chat_leave_confirm', N'zh-CN', N'确定要退出该群组吗？' UNION ALL

    SELECT N'chat_removed_from_group', N'en-US', N'You are no longer in this group' UNION ALL
    SELECT N'chat_removed_from_group', N'vi-VN', N'Bạn không còn trong nhóm này' UNION ALL
    SELECT N'chat_removed_from_group', N'zh-CN', N'您已不在该群组中' UNION ALL

    SELECT N'chat_sys_group_created', N'en-US', N'created the group' UNION ALL
    SELECT N'chat_sys_group_created', N'vi-VN', N'đã tạo nhóm' UNION ALL
    SELECT N'chat_sys_group_created', N'zh-CN', N'创建了群组' UNION ALL

    SELECT N'chat_sys_group_renamed', N'en-US', N'renamed the group' UNION ALL
    SELECT N'chat_sys_group_renamed', N'vi-VN', N'đã đổi tên nhóm' UNION ALL
    SELECT N'chat_sys_group_renamed', N'zh-CN', N'修改了群组名称' UNION ALL

    SELECT N'chat_sys_members_added', N'en-US', N'added members' UNION ALL
    SELECT N'chat_sys_members_added', N'vi-VN', N'đã thêm thành viên' UNION ALL
    SELECT N'chat_sys_members_added', N'zh-CN', N'添加了成员' UNION ALL

    SELECT N'chat_sys_member_removed', N'en-US', N'removed a member' UNION ALL
    SELECT N'chat_sys_member_removed', N'vi-VN', N'đã xoá một thành viên' UNION ALL
    SELECT N'chat_sys_member_removed', N'zh-CN', N'移除了一名成员' UNION ALL

    SELECT N'chat_sys_member_left', N'en-US', N'left the group' UNION ALL
    SELECT N'chat_sys_member_left', N'vi-VN', N'đã rời nhóm' UNION ALL
    SELECT N'chat_sys_member_left', N'zh-CN', N'退出了群组' UNION ALL

    SELECT N'chat_error_generic', N'en-US', N'Something went wrong, please try again' UNION ALL
    SELECT N'chat_error_generic', N'vi-VN', N'Có lỗi xảy ra, vui lòng thử lại' UNION ALL
    SELECT N'chat_error_generic', N'zh-CN', N'发生错误，请重试' UNION ALL

    SELECT N'chat_not_member', N'en-US', N'You do not have access to this conversation' UNION ALL
    SELECT N'chat_not_member', N'vi-VN', N'Bạn không có quyền truy cập hội thoại này' UNION ALL
    SELECT N'chat_not_member', N'zh-CN', N'您无权访问该会话' UNION ALL

    SELECT N'chat_message_empty', N'en-US', N'Message cannot be empty' UNION ALL
    SELECT N'chat_message_empty', N'vi-VN', N'Tin nhắn không được để trống' UNION ALL
    SELECT N'chat_message_empty', N'zh-CN', N'消息不能为空' UNION ALL

    SELECT N'chat_message_too_long', N'en-US', N'Message is too long (max 4000 characters)' UNION ALL
    SELECT N'chat_message_too_long', N'vi-VN', N'Tin nhắn quá dài (tối đa 4000 ký tự)' UNION ALL
    SELECT N'chat_message_too_long', N'zh-CN', N'消息过长（最多 4000 个字符）' UNION ALL

    SELECT N'chat_message_not_found', N'en-US', N'Message not found' UNION ALL
    SELECT N'chat_message_not_found', N'vi-VN', N'Không tìm thấy tin nhắn' UNION ALL
    SELECT N'chat_message_not_found', N'zh-CN', N'未找到消息' UNION ALL

    SELECT N'chat_attachment_too_large', N'en-US', N'File exceeds 25 MB' UNION ALL
    SELECT N'chat_attachment_too_large', N'vi-VN', N'File vượt quá 25 MB' UNION ALL
    SELECT N'chat_attachment_too_large', N'zh-CN', N'文件超过 25 MB' UNION ALL

    SELECT N'chat_attachment_name_required', N'en-US', N'File has no name' UNION ALL
    SELECT N'chat_attachment_name_required', N'vi-VN', N'File không có tên' UNION ALL
    SELECT N'chat_attachment_name_required', N'zh-CN', N'文件没有名称' UNION ALL

    SELECT N'chat_attachment_path_invalid', N'en-US', N'Invalid file path' UNION ALL
    SELECT N'chat_attachment_path_invalid', N'vi-VN', N'Đường dẫn file không hợp lệ' UNION ALL
    SELECT N'chat_attachment_path_invalid', N'zh-CN', N'文件路径无效' UNION ALL

    SELECT N'chat_user_not_found', N'en-US', N'User not found' UNION ALL
    SELECT N'chat_user_not_found', N'vi-VN', N'Không tìm thấy người dùng' UNION ALL
    SELECT N'chat_user_not_found', N'zh-CN', N'未找到用户' UNION ALL

    SELECT N'chat_group_name_required', N'en-US', N'Please enter a group name' UNION ALL
    SELECT N'chat_group_name_required', N'vi-VN', N'Vui lòng nhập tên nhóm' UNION ALL
    SELECT N'chat_group_name_required', N'zh-CN', N'请输入群组名称' UNION ALL

    SELECT N'chat_group_min_members', N'en-US', N'A group needs at least 2 other members' UNION ALL
    SELECT N'chat_group_min_members', N'vi-VN', N'Nhóm cần ít nhất 2 thành viên khác' UNION ALL
    SELECT N'chat_group_min_members', N'zh-CN', N'群组至少需要另外 2 名成员' UNION ALL

    SELECT N'chat_group_only', N'en-US', N'Only available for groups' UNION ALL
    SELECT N'chat_group_only', N'vi-VN', N'Chỉ áp dụng cho nhóm' UNION ALL
    SELECT N'chat_group_only', N'zh-CN', N'仅适用于群组' UNION ALL

    SELECT N'chat_owner_only', N'en-US', N'Only the group owner can do this' UNION ALL
    SELECT N'chat_owner_only', N'vi-VN', N'Chỉ trưởng nhóm được thực hiện' UNION ALL
    SELECT N'chat_owner_only', N'zh-CN', N'仅群主可执行此操作' UNION ALL

    SELECT N'chat_cannot_remove_self', N'en-US', N'Use ''Leave group'' to remove yourself' UNION ALL
    SELECT N'chat_cannot_remove_self', N'vi-VN', N'Dùng ''Rời nhóm'' để tự rời khỏi nhóm' UNION ALL
    SELECT N'chat_cannot_remove_self', N'zh-CN', N'请使用“退出群组”来退出' UNION ALL

    SELECT N'chat_more', N'en-US', N'More' UNION ALL
    SELECT N'chat_more', N'vi-VN', N'Thêm' UNION ALL
    SELECT N'chat_more', N'zh-CN', N'更多' UNION ALL

    SELECT N'chat_recall', N'en-US', N'Unsend' UNION ALL
    SELECT N'chat_recall', N'vi-VN', N'Thu hồi' UNION ALL
    SELECT N'chat_recall', N'zh-CN', N'撤回' UNION ALL

    SELECT N'chat_recall_confirm', N'en-US', N'Unsend this message for everyone? Its content and file will be permanently deleted.' UNION ALL
    SELECT N'chat_recall_confirm', N'vi-VN', N'Thu hồi tin nhắn này với mọi người? Nội dung và file sẽ bị xoá vĩnh viễn.' UNION ALL
    SELECT N'chat_recall_confirm', N'zh-CN', N'要为所有人撤回此消息吗？内容和文件将被永久删除。' UNION ALL

    SELECT N'chat_message_recalled', N'en-US', N'This message was unsent' UNION ALL
    SELECT N'chat_message_recalled', N'vi-VN', N'Tin nhắn đã được thu hồi' UNION ALL
    SELECT N'chat_message_recalled', N'zh-CN', N'消息已撤回' UNION ALL

    SELECT N'chat_recall_expired', N'en-US', N'Messages can only be unsent within 24 hours' UNION ALL
    SELECT N'chat_recall_expired', N'vi-VN', N'Chỉ thu hồi được tin nhắn trong vòng 24 giờ' UNION ALL
    SELECT N'chat_recall_expired', N'zh-CN', N'只能撤回 24 小时内的消息' UNION ALL

    SELECT N'chat_recall_not_allowed', N'en-US', N'You can only unsend your own messages' UNION ALL
    SELECT N'chat_recall_not_allowed', N'vi-VN', N'Bạn chỉ thu hồi được tin nhắn của mình' UNION ALL
    SELECT N'chat_recall_not_allowed', N'zh-CN', N'只能撤回自己的消息' UNION ALL

    SELECT N'chat_hide_conversation', N'en-US', N'Delete conversation' UNION ALL
    SELECT N'chat_hide_conversation', N'vi-VN', N'Xoá hội thoại' UNION ALL
    SELECT N'chat_hide_conversation', N'zh-CN', N'删除会话' UNION ALL

    SELECT N'chat_hide_confirm', N'en-US', N'Delete this conversation for you? Others keep their history. It will reappear when a new message arrives.' UNION ALL
    SELECT N'chat_hide_confirm', N'vi-VN', N'Xoá hội thoại này phía bạn? Người khác vẫn giữ nguyên lịch sử. Hội thoại sẽ hiện lại khi có tin mới.' UNION ALL
    SELECT N'chat_hide_confirm', N'zh-CN', N'为您删除此会话？其他人的记录保持不变。收到新消息时会话将重新出现。' UNION ALL

    SELECT N'chat_disband_group', N'en-US', N'Disband group' UNION ALL
    SELECT N'chat_disband_group', N'vi-VN', N'Giải tán nhóm' UNION ALL
    SELECT N'chat_disband_group', N'zh-CN', N'解散群组' UNION ALL

    SELECT N'chat_disband_confirm', N'en-US', N'Disband the group and delete all its messages and files for every member? This cannot be undone.' UNION ALL
    SELECT N'chat_disband_confirm', N'vi-VN', N'Giải tán nhóm và xoá toàn bộ tin nhắn, file của nhóm cho mọi thành viên? Không thể hoàn tác.' UNION ALL
    SELECT N'chat_disband_confirm', N'zh-CN', N'解散群组并为所有成员删除全部消息和文件？此操作无法撤销。' UNION ALL

    SELECT N'chat_group_disbanded', N'en-US', N'The group has been disbanded' UNION ALL
    SELECT N'chat_group_disbanded', N'vi-VN', N'Nhóm đã bị giải tán' UNION ALL
    SELECT N'chat_group_disbanded', N'zh-CN', N'群组已解散' UNION ALL

    SELECT N'chat_reply', N'en-US', N'Reply' UNION ALL
    SELECT N'chat_reply', N'vi-VN', N'Trả lời' UNION ALL
    SELECT N'chat_reply', N'zh-CN', N'回复' UNION ALL

    SELECT N'chat_replying_to', N'en-US', N'Replying to' UNION ALL
    SELECT N'chat_replying_to', N'vi-VN', N'Đang trả lời' UNION ALL
    SELECT N'chat_replying_to', N'zh-CN', N'正在回复' UNION ALL

    SELECT N'chat_forward', N'en-US', N'Forward' UNION ALL
    SELECT N'chat_forward', N'vi-VN', N'Chuyển tiếp' UNION ALL
    SELECT N'chat_forward', N'zh-CN', N'转发' UNION ALL

    SELECT N'chat_forwarded', N'en-US', N'Forwarded' UNION ALL
    SELECT N'chat_forwarded', N'vi-VN', N'Chuyển tiếp' UNION ALL
    SELECT N'chat_forwarded', N'zh-CN', N'已转发' UNION ALL

    SELECT N'chat_forward_done', N'en-US', N'Forwarded' UNION ALL
    SELECT N'chat_forward_done', N'vi-VN', N'Đã chuyển tiếp' UNION ALL
    SELECT N'chat_forward_done', N'zh-CN', N'已转发' UNION ALL

    SELECT N'chat_forward_limit', N'en-US', N'You can forward to at most 10 destinations at a time' UNION ALL
    SELECT N'chat_forward_limit', N'vi-VN', N'Chỉ chuyển tiếp tối đa 10 nơi mỗi lần' UNION ALL
    SELECT N'chat_forward_limit', N'zh-CN', N'每次最多转发给 10 个对象' UNION ALL

    SELECT N'chat_forward_no_target', N'en-US', N'Please select at least one destination' UNION ALL
    SELECT N'chat_forward_no_target', N'vi-VN', N'Vui lòng chọn nơi nhận' UNION ALL
    SELECT N'chat_forward_no_target', N'zh-CN', N'请选择接收对象' UNION ALL

    SELECT N'chat_forward_not_allowed', N'en-US', N'This message cannot be forwarded' UNION ALL
    SELECT N'chat_forward_not_allowed', N'vi-VN', N'Không thể chuyển tiếp tin nhắn này' UNION ALL
    SELECT N'chat_forward_not_allowed', N'zh-CN', N'无法转发此消息' UNION ALL

    SELECT N'chat_recent_conversations', N'en-US', N'Recent conversations' UNION ALL
    SELECT N'chat_recent_conversations', N'vi-VN', N'Hội thoại gần đây' UNION ALL
    SELECT N'chat_recent_conversations', N'zh-CN', N'最近会话' UNION ALL

    SELECT N'chat_users', N'en-US', N'Users' UNION ALL
    SELECT N'chat_users', N'vi-VN', N'Người dùng' UNION ALL
    SELECT N'chat_users', N'zh-CN', N'用户' UNION ALL

    SELECT N'chat_pin', N'en-US', N'Pin' UNION ALL
    SELECT N'chat_pin', N'vi-VN', N'Ghim' UNION ALL
    SELECT N'chat_pin', N'zh-CN', N'置顶' UNION ALL

    SELECT N'chat_unpin', N'en-US', N'Unpin' UNION ALL
    SELECT N'chat_unpin', N'vi-VN', N'Bỏ ghim' UNION ALL
    SELECT N'chat_unpin', N'zh-CN', N'取消置顶' UNION ALL

    SELECT N'chat_pinned_messages', N'en-US', N'Pinned messages' UNION ALL
    SELECT N'chat_pinned_messages', N'vi-VN', N'Tin nhắn đã ghim' UNION ALL
    SELECT N'chat_pinned_messages', N'zh-CN', N'置顶消息' UNION ALL

    SELECT N'chat_pin_not_allowed', N'en-US', N'This message cannot be pinned' UNION ALL
    SELECT N'chat_pin_not_allowed', N'vi-VN', N'Không thể ghim tin nhắn này' UNION ALL
    SELECT N'chat_pin_not_allowed', N'zh-CN', N'无法置顶此消息' UNION ALL

    SELECT N'chat_sys_pinned', N'en-US', N'pinned a message' UNION ALL
    SELECT N'chat_sys_pinned', N'vi-VN', N'đã ghim một tin nhắn' UNION ALL
    SELECT N'chat_sys_pinned', N'zh-CN', N'置顶了一条消息' UNION ALL

    SELECT N'chat_sys_unpinned', N'en-US', N'unpinned a message' UNION ALL
    SELECT N'chat_sys_unpinned', N'vi-VN', N'đã bỏ ghim một tin nhắn' UNION ALL
    SELECT N'chat_sys_unpinned', N'zh-CN', N'取消置顶了一条消息' UNION ALL

    SELECT N'chat_edit', N'en-US', N'Edit' UNION ALL
    SELECT N'chat_edit', N'vi-VN', N'Chỉnh sửa' UNION ALL
    SELECT N'chat_edit', N'zh-CN', N'编辑' UNION ALL

    SELECT N'chat_editing', N'en-US', N'Editing message' UNION ALL
    SELECT N'chat_editing', N'vi-VN', N'Đang chỉnh sửa tin nhắn' UNION ALL
    SELECT N'chat_editing', N'zh-CN', N'正在编辑消息' UNION ALL

    SELECT N'chat_edited', N'en-US', N'edited' UNION ALL
    SELECT N'chat_edited', N'vi-VN', N'đã chỉnh sửa' UNION ALL
    SELECT N'chat_edited', N'zh-CN', N'已编辑' UNION ALL

    SELECT N'chat_edit_expired', N'en-US', N'Messages can only be edited within 24 hours' UNION ALL
    SELECT N'chat_edit_expired', N'vi-VN', N'Chỉ chỉnh sửa được tin nhắn trong vòng 24 giờ' UNION ALL
    SELECT N'chat_edit_expired', N'zh-CN', N'只能编辑 24 小时内的消息' UNION ALL

    SELECT N'chat_edit_not_allowed', N'en-US', N'You can only edit your own text messages' UNION ALL
    SELECT N'chat_edit_not_allowed', N'vi-VN', N'Bạn chỉ chỉnh sửa được tin nhắn chữ của mình' UNION ALL
    SELECT N'chat_edit_not_allowed', N'zh-CN', N'只能编辑自己的文字消息'
)
MERGE dbo.LocalizationResources AS tgt
USING src
ON tgt.ResourceKey = src.ResourceKey AND tgt.Culture = src.Culture
WHEN MATCHED THEN
    UPDATE SET Value = src.Value
WHEN NOT MATCHED BY TARGET THEN
    INSERT (ResourceKey, Culture, Value)
    VALUES (src.ResourceKey, src.Culture, src.Value);

COMMIT TRANSACTION;

PRINT N'Chat localization imported.';
